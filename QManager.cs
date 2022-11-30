using Newtonsoft.Json;
using QEngine.Core.EventArgs;
using QEngine.Core.Exceptions;
using System.Text;
using static QEngine.Core.EventArgs.QEventHandlers;

namespace QEngine.Core
{
    public class QManager
    {
        public event QueueChangedEventHandler? QueueChanged;
        public event ClientChangedEventHandler? QueueClientChanged;

        public event QueueItemChangedEventHandler? QueueItemChanged;
        public event SubscribersChangedEventHandler? QueueSubscriberChanged;
        public event QueueThrottlingEventHandler? QueueItemRejected;
        public event QueueThrottlingEventHandler? QueueItemIgnored;

        public event QueueBehaviourChangedEventHandler? QueueBehaviourChanged;

        //SignalR ConnectionID and SignalRClient Info
        private Dictionary<string, QClientInfo> _qEngineClients = new();

        //QueueName, Queue Entity
        private Dictionary<string, Q> _queueNames = new();
        private Dictionary<string, (bool, DateTime, DateTime)> _qItemCommittedDurations = new();

        private string _queueStorage = "queues.dat";

        public QManager()
        {
            if (!File.Exists(_queueStorage))
                File.CreateText(_queueStorage).Close();

            Open();
        }

        public int GetItemsCount(string queueName = "")
        {
            if (string.IsNullOrWhiteSpace(queueName))
                return _queueNames.Values.Sum(x => x.ItemCount);

            if (_queueNames.ContainsKey(queueName))
                return _queueNames[queueName].ItemCount;

            return 0;
        }

        public int GetSubscriberCount(string queueName = "")
        {
            if (string.IsNullOrWhiteSpace(queueName))
                return _queueNames.Values.Sum(x => x.SubscriberCount);

            if (_queueNames.ContainsKey(queueName))
                return _queueNames[queueName].SubscriberCount;

            return 0;
        }

        public int GetClientCount(QOrigin type = QOrigin.All)
        {
            switch (type)
            {
                case QOrigin.All:
                    return _qEngineClients.Count;
                case QOrigin.Consumer:
                case QOrigin.Subscriber:
                case QOrigin.Producer:
                default:
                    return _qEngineClients.Values.Count(x => x.Type == type);
            }
        }

        public Q[] Queues
        {
            get
            {
                return _queueNames.Values.ToArray();
            }
        }

        public Q this[string name]
        {
            get
            {
                if (_queueNames.ContainsKey(name))
                    return _queueNames[name];

                throw new QNotExistsException(name);
            }
        }

        public (bool, string) RequestCancelOperation(string connectionId, string queueName)
        {
            if (!Exists(queueName))
                return (false, "Queue name not exist.");

            if (this[queueName].CancelRequest(connectionId))
                return (true, "Cancel request accepted.");

            return (false, "Cancel request ignored.");
        }

        #region Queue List Operation - Create, Alter, Remove, Exists

        public void Create(string queueName)
        {
            if (_queueNames.ContainsKey(queueName))
                throw new ArgumentException($"Queue Name {queueName} already exists.");

            var queue = new Q(queueName);
            queue.ItemChanged += QueueItemChanged;
            queue.SubscribersChanged += QueueSubscriberChanged;
            _queueNames.Add(queueName, queue);
            QueueChanged?.Invoke(new QChangedEventArgs(queueName, queue, MethodName.Create));
            NotifyQueueChanges(MethodName.Create);
            Save();
        }

        public void Create(string queueName, Q queue)
        {
            if (_queueNames.ContainsKey(queueName))
                throw new ArgumentException($"Queue Name {queueName} already exists.");

            queue.ItemChanged += QueueItemChanged;
            queue.SubscribersChanged += QueueSubscriberChanged;
            _queueNames.Add(queueName, queue);
            QueueChanged?.Invoke(new QChangedEventArgs(queueName, queue, MethodName.Create));
            NotifyQueueChanges(MethodName.Create);
            Save();
        }

        public bool Exists(string queueName)
        {
            return _queueNames.ContainsKey(queueName);
        }

        public bool Alter(string queueName, QBehaviours behaviour)
        {
            if (!_queueNames.ContainsKey(queueName))
                throw new ArgumentException($"Queue Name {queueName} is not exists.");

            var queue = _queueNames[queueName];
            QBehavioursChangedEventArgs qbe = new(queueName, MethodName.Alter);
            if (queue.Behaviours.ProduceBehaviour != behaviour.ProduceBehaviour)
                qbe.Changes.Add("ProduceBehaviour", (queue.Behaviours.ProduceBehaviour, behaviour.ProduceBehaviour));
            if (queue.Behaviours.ProduceTimeOut != behaviour.ProduceTimeOut)
                qbe.Changes.Add("ProduceTimeOut", (queue.Behaviours.ProduceTimeOut, behaviour.ProduceTimeOut));
            if (queue.Behaviours.ConsumeBehaviour != behaviour.ConsumeBehaviour)
                qbe.Changes.Add("ConsumeBehaviour", (queue.Behaviours.ConsumeBehaviour, behaviour.ConsumeBehaviour));
            if (queue.Behaviours.ConsumeTimeOut != behaviour.ConsumeTimeOut)
                qbe.Changes.Add("ConsumeTimeOut", (queue.Behaviours.ConsumeTimeOut, behaviour.ConsumeTimeOut));

            if (qbe.Changes.Count > 0) QueueBehaviourChanged?.Invoke(qbe);

            queue.Behaviours = behaviour;
            QueueChanged?.Invoke(new QChangedEventArgs(queueName, queue, MethodName.Alter));
            NotifyQueueChanges(MethodName.Alter);
            Save();
            return true;
        }

        public bool Remove(string queueName)
        {
            if (!_queueNames.ContainsKey(queueName))
                throw new ArgumentException($"Queue Name {queueName} is not exists.");

            var queue = _queueNames[queueName];
            queue.ItemChanged -= QueueItemChanged;
            queue.SubscribersChanged -= QueueSubscriberChanged;
            _queueNames.Remove(queueName);
            QueueChanged?.Invoke(new QChangedEventArgs(queueName, queue, MethodName.Remove));
            NotifyQueueChanges(MethodName.Remove);
            Save();
            return true;
        }

        private async void NotifyQueueChanges(MethodName methodName)
        {
            switch (methodName)
            {
                case MethodName.Create:
                    await SaveAsync();
                    break;
                case MethodName.Alter:
                    break;
                case MethodName.Remove:
                    break;
            }
        }

        #endregion

        #region Queue List IO - Open & Save 

        private void Open()
        {
            using var _fileStream = new FileStream(_queueStorage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var sb = new StringBuilder();
            var buffer = new byte[1024];
            int read;
            while ((read = _fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var temp = new byte[read];
                Array.Copy(buffer, temp, read);
                sb.Append(Encoding.UTF8.GetString(temp));
            }
            var json = sb.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                _queueNames = new Dictionary<string, Q>();
                Save();
            }
            else
            {
                _queueNames = JsonConvert.DeserializeObject<Dictionary<string, Q>>(sb.ToString(), new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                }) ?? new Dictionary<string, Q>();
            }
        }

        private async Task OpenAsync()
        {
            using var _fileStream = new FileStream(_queueStorage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var sb = new StringBuilder();
            var buffer = new byte[1024];
            int read;
            while ((read = await _fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var temp = new byte[read];
                Array.Copy(buffer, temp, read);
                sb.Append(Encoding.UTF8.GetString(buffer));
            }
            _queueNames = JsonConvert.DeserializeObject<Dictionary<string, Q>>(sb.ToString(), new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            }) ?? new Dictionary<string, Q>();
        }

        private void Save()
        {
            using var _fileStream = new FileStream(_queueStorage, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            var queueNamesJson = JsonConvert.SerializeObject(_queueNames, Formatting.Indented);
            var queueNamesJsonBytes = Encoding.UTF8.GetBytes(queueNamesJson);
            _fileStream.Position = 0;
            _fileStream.Write(queueNamesJsonBytes);
            _fileStream.SetLength(queueNamesJsonBytes.Length);
            _fileStream.Flush();
        }

        private async Task SaveAsync()
        {
            using var _fileStream = new FileStream(_queueStorage, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            var queueNamesJson = JsonConvert.SerializeObject(_queueNames);
            var queueNamesJsonBytes = Encoding.UTF8.GetBytes(queueNamesJson);
            _fileStream.Position = 0;
            await _fileStream.WriteAsync(queueNamesJsonBytes);
            _fileStream.SetLength(queueNamesJsonBytes.Length);
            await _fileStream.FlushAsync();
        }

        #endregion

        #region Register & Unregister Events for Server Side

        public void RegisterEvent()
        {
            foreach (var q in _queueNames.Values)
            {
                q.SubscribersChanged += QueueSubscriberChanged;
                q.ItemChanged += QueueItemChanged;
                q.ItemRejected += QueueItemRejected;
                q.ItemIgnored += QueueItemIgnored;
            }
        }

        public void UnregisterEvent()
        {
            foreach (var q in _queueNames.Values)
            {
                q.SubscribersChanged -= QueueSubscriberChanged;
                q.ItemChanged -= QueueItemChanged;
                q.ItemRejected -= QueueItemRejected;
                q.ItemIgnored -= QueueItemIgnored;
            }
        }

        #endregion

        #region SignalR Clients

        public QClientInfo[] Clients
        {
            get
            {
                return _qEngineClients.Values.ToArray();
            }
        }

        internal void AddClient(string connectionId, QClientInfo client)
        {
            if (!_qEngineClients.ContainsKey(connectionId))
            {
                var e = new QClientEventArgs(connectionId, client, _qEngineClients.Count, 0);
                _qEngineClients.TryAdd(connectionId, client);
                e.NewCount = _qEngineClients.Count;
                QueueClientChanged?.Invoke(e);
            }
        }

        internal void UpdateEchoClient(string connectionId, DateTime time)
        {
            if (_qEngineClients.ContainsKey(connectionId))
                _qEngineClients[connectionId].LastEchoTime = time;
        }

        internal bool IsClientExists(string connectionId)
        {
            return _qEngineClients.ContainsKey(connectionId);
        }

        internal void RemoveClient(string connectionId)
        {
            if (_qEngineClients.ContainsKey(connectionId))
            {
                var client = _qEngineClients[connectionId];
                var e = new QClientEventArgs(connectionId, client, _qEngineClients.Count, 0);
                _qEngineClients.Remove(connectionId);
                e.NewCount = _qEngineClients.Count;
                QueueClientChanged?.Invoke(e);
            }
        }

        internal QClientInfo? GetClient(string connectionId)
        {
            if (_qEngineClients.ContainsKey(connectionId))
                return _qEngineClients[connectionId];

            return null;
        }

        internal string[] ClientConnectionIds
        {
            get
            {
                return _qEngineClients.Keys.ToArray();
            }
        }

        internal void SetClient(string connectionId, QClientInfo client)
        {
            QueueClientChanged?.Invoke(new QClientEventArgs(connectionId, client, _qEngineClients.Count, _qEngineClients.Count));
            if (!_qEngineClients.ContainsKey(connectionId))
            {
                AddClient(connectionId, client);
                return;
            }

            _qEngineClients[connectionId] = client;
        }

        #endregion
    }
}