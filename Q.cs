using Lifecare.Helper;
using Newtonsoft.Json;
using QEngine.Core.Abstractions;
using QEngine.Core.EventArgs;
using QEngine.Core.Storages;
using QEngine.Core.Values;
using System.Collections.Concurrent;
using System.Diagnostics;
using static QEngine.Core.EventArgs.QEventHandlers;

namespace QEngine.Core
{
    /// <summary>
    /// As queue to store QItem within the BlockingCollection to ensure thread safe
    /// Q is serializable to Json Format that stored in a file queues.dat
    /// </summary>
    public class Q
    {
        private readonly Dictionary<string, CancellationTokenSource> _subscribers = new();
        private readonly Dictionary<string, CancellationTokenSource> _producers = new();

        /// <summary>
        /// Triggered when collection of item has changed
        /// </summary>
        public event QueueItemChangedEventHandler? ItemChanged;
        /// <summary>
        /// Triggered when collection of item is full or at max (capacity) and inform producers that item is rejected
        /// </summary>
        public event QueueThrottlingEventHandler? ItemRejected;
        /// <summary>
        /// Triggered when collection of item is full or at max (capacity) and inform producers that item is ignored
        /// </summary>
        public event QueueThrottlingEventHandler? ItemIgnored;
        /// <summary>
        /// Triggered when collection of subscriber has changed
        /// </summary>
        public event SubscribersChangedEventHandler? SubscribersChanged;
        /// <summary>
        /// Triggered when collection of producer has changed
        /// </summary>
        public event BindersChangedEventHandler? BindersChanged;

        private BlockingCollection<QItem> _queues;
        private uint _capacity;

        private IStorage _storage;

        public Q(string name, uint capacity = 0)
        {
            Id = Randomizer.Generate(15);

            _subscribers = new();
            _producers = new();

            Storage = new MemoryStorage();

            Name = name;
            Capacity = capacity;

            Properties = new();
            Behaviours = new();

            if (capacity > 0)
                _queues = new((int)capacity);
            else
                _queues = new();

            Storage = (IStorage)Activator.CreateInstance(StorageType)!;
        }

        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public uint Capacity
        {
            get { return _capacity; }
            set
            {
                _capacity = value;
                if (_capacity > 0)
                    _queues = new((int)_capacity);
                else
                    _queues = new();
            }
        }

        public Type StorageType { get; set; } = typeof(MemoryStorage);

        [JsonIgnore]
        public IStorage Storage
        {
            get
            {
                return _storage;
            }
            set
            {
                _storage = value;
                if (value != null)
                {
                    StorageType = _storage.GetType();
                }
            }
        }

        public QBehaviours Behaviours { get; set; }

        public QProperties Properties { get; set; }

        public int ItemCount => _queues.Count;

        public int SubscriberCount => _subscribers.Count;

        /// <summary>
        /// Bind to queue
        /// </summary>
        /// <param name="connectionId">Connection Id received from client only from QProducer</param>
        public void Bind(string connectionId)
        {
            if (!_producers.ContainsKey(connectionId))
            {
                var e = new QBinderEventArgs(connectionId, _producers.Count, 0);
                var cts = new CancellationTokenSource();
                cts.Token.ThrowIfCancellationRequested();
                _producers.Add(connectionId, cts);
                e.NewCount = _producers.Count;
                BindersChanged?.Invoke(e);
            }
        }

        /// <summary>
        /// Unbind queue
        /// </summary>
        /// <param name="connectionId">Connection Id received from client only from QProducer</param>
        public void Unbind(string connectionId)
        {
            if (_producers.ContainsKey(connectionId))
            {
                var e = new QBinderEventArgs(connectionId, _producers.Count, 0);
                _producers.Remove(connectionId);
                e.NewCount = _producers.Count;
                BindersChanged?.Invoke(e);
            }
        }

        /// <summary>
        /// Subscribe to queue
        /// </summary>
        /// <param name="connectionId">Connection Id received from client only from QConsumer or QSubscriber</param>
        public void Subscribe(string connectionId)
        {
            if (!_subscribers.ContainsKey(connectionId))
            {
                var e = new QSubscriberEventArgs(connectionId, _subscribers.Count, 0);
                var cts = new CancellationTokenSource();
                cts.Token.ThrowIfCancellationRequested();
                _subscribers.Add(connectionId, cts);
                e.NewCount = _subscribers.Count;
                SubscribersChanged?.Invoke(e);
            }
        }

        /// <summary>
        /// Unsubscribe queue
        /// </summary>
        /// <param name="connectionId">Connection Id received from client only from QConsumer or QSubscriber</param>
        public void Unsubscribe(string connectionId)
        {
            if (_subscribers.ContainsKey(connectionId))
            {
                var e = new QSubscriberEventArgs(connectionId, _subscribers.Count, 0);
                _subscribers.Remove(connectionId);
                e.NewCount = _subscribers.Count;
                SubscribersChanged?.Invoke(e);
            }
        }

        /// <summary>
        /// Produce/Add QItem in to queue item collection
        /// </summary>
        /// <param name="data">QItem to be added</param>
        /// <param name="timeOutMs">Timeout in milliseconds, set to -1 to wait indefinitely</param>
        /// <returns>Return QProduceResult with props IsSucceeded and Throttling behaviour. IsSucceeded set to true if item added to collection,
        /// otherwise false.</returns>
        public QProduceResult Produce(string connectionId, QItem data, int timeOutMs = -1)
        {
            QProduceResult produceResult = new();
            CancellationToken cancellationToken = _producers[connectionId].Token;
            Stopwatch stopWatch = new();
            stopWatch.Start();
            switch (Behaviours.ProduceBehaviour)
            {
                case QThrottling.Block:
                case QThrottling.BlockWithTimeOut:
                    try
                    {
                        produceResult.IsSucceeded = timeOutMs switch
                        {
                            -1 => _queues.TryAdd(data, Behaviours.ProduceTimeOut, cancellationToken),
                            _ => _queues.TryAdd(data, timeOutMs, cancellationToken),
                        };

                        var elapsedMs = stopWatch.ElapsedMilliseconds;
                        if (produceResult.IsSucceeded)
                        {
                            produceResult.Item = data;
                            ItemChanged?.Invoke(new QItemEventArgs(Name, data, QMethodName.Produce));
                            return produceResult;
                        }

                        if (elapsedMs >= timeOutMs && timeOutMs != -1)
                        {
                            produceResult.Item = new QItem(new TimeOutValue(timeOutMs));
                            return produceResult;
                        }
                    }
                    catch when (cancellationToken.IsCancellationRequested)
                    {
                        produceResult.IsSucceeded = false;
                        produceResult.Item = new QItem(new RequestCancelValue());
                        return produceResult;
                    }
                    catch (Exception e)
                    {
                        produceResult.IsSucceeded = false;
                        produceResult.Item = new QItem(new ExceptionValue(e.Message));
                        return produceResult;
                    }
                    finally
                    {
                        if (stopWatch.IsRunning)
                            stopWatch.Stop();
                    }

                    break;
                case QThrottling.Ignore:
                    produceResult.IsSucceeded = _queues.TryAdd(data, 0, cancellationToken);
                    if (!produceResult.IsSucceeded)
                    {
                        produceResult.Throttling = QThrottling.Ignore;
                        ItemIgnored?.Invoke(new QThrottlingEventArgs(Name, data, QOrigin.Producer, QThrottling.Ignore));
                    }
                    break;
                case QThrottling.Reject:
                    produceResult.IsSucceeded = _queues.TryAdd(data, 0, cancellationToken);
                    if (!produceResult.IsSucceeded)
                    {
                        produceResult.Throttling = QThrottling.Reject;
                        ItemRejected?.Invoke(new QThrottlingEventArgs(Name, data, QOrigin.Producer, QThrottling.Reject));
                    }
                    break;
                default:
                    break;
            }

            if (Storage is not null && produceResult.IsSucceeded)
            {
                //If Queue Persistent is true and data Options Persistent is true
                if (Properties.Persistent && data.Options.Persistent)
                    Storage.Add(data);
            }

            if (produceResult.IsSucceeded)
                ItemChanged?.Invoke(new QItemEventArgs(Name, data, QMethodName.Produce));

            return produceResult;
        }

        /// <summary>
        /// Consume QItem from queue item collection
        /// </summary>
        /// <param name="timeOutMs">Timeout in milliseconds, set to -1 to wait indefinitely</param>
        /// <returns>Return QConsumeResult with props IsSucceeded and the item. IsSucceeded set to true if item consumed from collection,
        /// otherwise false.</returns>
        public QConsumeResult Consume(string connectionId, int timeOutMs = -1)
        {
            QConsumeResult consumeResult = new();
            CancellationToken cancellationToken = _subscribers[connectionId].Token;
            Stopwatch stopWatch = new();
            stopWatch.Start();
            switch (Behaviours.ConsumeBehaviour)
            {
                case QThrottling.Block:
                case QThrottling.BlockWithTimeOut:
                    try
                    {
                        QItem item = new();
                        consumeResult.IsSucceeded = timeOutMs switch
                        {
                            -1 => _queues.TryTake(out item, Behaviours.ConsumeTimeOut, cancellationToken),
                            _ => _queues.TryTake(out item, timeOutMs, cancellationToken),
                        };

                        var elapsedMs = stopWatch.ElapsedMilliseconds;
                        if (consumeResult.IsSucceeded)
                        {
                            consumeResult.Item = item;
                            ItemChanged?.Invoke(new QItemEventArgs(Name, item, QMethodName.Consume));
                            return consumeResult;
                        }

                        if (elapsedMs >= timeOutMs && timeOutMs != -1)
                        {
                            consumeResult.Item = new QItem(new TimeOutValue(timeOutMs));
                            return consumeResult;
                        }

                        break;
                    }
                    catch when (cancellationToken.IsCancellationRequested)
                    {
                        consumeResult.IsSucceeded = false;
                        consumeResult.Item = new QItem(new RequestCancelValue());
                        return consumeResult;
                    }
                    catch (Exception e)
                    {
                        consumeResult.IsSucceeded = false;
                        consumeResult.Item = new QItem(new ExceptionValue(e.Message));
                        return consumeResult;
                    }
                    finally
                    {
                        if (stopWatch.IsRunning)
                            stopWatch.Stop();
                    }
                case QThrottling.Ignore:
                case QThrottling.Reject:
                default:
                    break;
            }

            return consumeResult;
        }

        /// <summary>
        /// Gracefully terminate request for consume or produce when blocked
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public bool CancelRequest(string connectionId)
        {
            try
            {
                if (_subscribers.ContainsKey(connectionId))
                {
                    _subscribers[connectionId].Cancel();
                    Console.WriteLine($"Cancel consume request from {connectionId}");
                }

                if (_producers.ContainsKey(connectionId))
                {
                    _producers[connectionId].Cancel();
                    Console.WriteLine($"Cancel produce request from {connectionId}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Acknowledge item that has been produced
        /// </summary>
        /// <param name="data"></param>
        public void Acknowledge(QItem data)
        {
            if (Storage is not null)
            {
                var existingData = Storage.Get(data.Id);
                if (!existingData.IsNullOrEmpty)
                    Storage.Acknowledge(data.Id);
            }
        }

        /// <summary>
        /// Commit item that has been consumed
        /// </summary>
        /// <param name="data"></param>
        public void Commit(QItem data)
        {
            if (Storage is not null)
            {
                var existingData = Storage.Get(data.Id);
                if (!existingData.IsNullOrEmpty)
                    Storage.Commit(data.Id, data.CommitedBy);
            }
        }

        public void Delete(QItem data)
        {
            if (Storage is not null)
                Storage.Remove(data.Id);
        }

        [JsonIgnore]
        public IEnumerable<QItem> Items
        {
            get
            {
                return Storage is not null ? Storage.Items : Array.Empty<QItem>();
            }
        }
    }
}