using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using QEngine.Core.Abstractions;
using QEngine.Core.EventArgs;
using QEngine.Core.Exceptions;
using QEngine.Core.Monitoring;

namespace QEngine.Core.Server
{
    public class QEngineHub : Hub<IQEngineHub>, IAsyncDisposable
    {
        private readonly QManager _qMgr;
        private readonly QMonitor _qMtr;

        public QEngineHub(QManager queueMgr, QMonitor qMtr)
        {
            _qMgr = queueMgr;
            _qMtr = qMtr;
        }

        public override async Task OnConnectedAsync()
        {
            var feature = Context.Features.Get<IHttpConnectionFeature>();
            _qMgr.AddClient(Context.ConnectionId, new QClientInfo { Id = Context.ConnectionId, Origin = feature.RemoteIpAddress.ToString() });
            await base.OnConnectedAsync();
            Console.WriteLine($"Connected: {Context.ConnectionId} => {_qMgr.IsClientExists(Context.ConnectionId)}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var clientInfo = _qMgr.GetClient(Context.ConnectionId);
            if (clientInfo != null && (clientInfo.Type == QOrigin.Subscriber || clientInfo.Type == QOrigin.Consumer))
            {
                _qMgr[clientInfo.QueueName].Unsubscribe(Context.ConnectionId);
                Console.WriteLine($"Unsubscribe on disconnected for {Context.ConnectionId} of {clientInfo.QueueName}");
            }

            if (clientInfo != null && (clientInfo.Type == QOrigin.Producer))
            {
                _qMgr[clientInfo.QueueName].Unbind(Context.ConnectionId);
                Console.WriteLine($"Unbind on disconnected for {Context.ConnectionId} of {clientInfo.QueueName}");
            }

            _qMgr.RemoveClient(Context.ConnectionId);
            Console.WriteLine($"Disconnected: {Context.ConnectionId} => {_qMgr.IsClientExists(Context.ConnectionId)}");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> AssignAsync(string name, string qEngineType)
        {
            var clientInfo = _qMgr.GetClient(Context.ConnectionId);
            if (clientInfo != null)
            {
                clientInfo.Name = name;
                clientInfo.Type = (QOrigin)Enum.Parse(typeof(QOrigin), qEngineType);
                _qMgr.SetClient(Context.ConnectionId, clientInfo);

                await Clients.Caller.OnAssigned();
                return true;
            }

            return false;
        }

        public async Task EchoAsync()
        {
            _qMgr.UpdateEchoClient(Context.ConnectionId, DateTime.Now);
            await Task.CompletedTask;
        }

        public async Task GetServerTime()
        {
            await Clients.Caller.OnBrokerTimeReceived(DateTime.Now);
        }

        public Task<int> GetSubscriberCount(string queueName)
        {
            return Task.FromResult(_qMgr[queueName]?.SubscriberCount ?? 0);
        }

        public Task<int> GetTotalSubscriberCount()
        {
            return Task.FromResult(_qMgr.GetSubscriberCount());
        }

        public Task<int> GetQueueItemCount(string queueName)
        {
            return Task.FromResult(_qMgr[queueName]?.ItemCount ?? 0);
        }

        public Task<Q[]> GetQueues()
        {
            return Task.FromResult(_qMgr.Queues);
        }

        public Task<int> GetClientCount(QOrigin qEngineType)
        {
            return Task.FromResult(_qMgr.GetClientCount(qEngineType));
        }

        public async Task<QItem> ConsumeAsync(string queueName, int timeOutMs = -1)
        {
            if (!_qMgr.Exists(queueName))
                throw new QNotExistsException(queueName);

            var q = _qMgr[queueName];
            var consumeResult = q.Behaviours.ConsumeBehaviour switch
            {
                QThrottling.Block => q.Consume(Context.ConnectionId),
                QThrottling.Ignore => throw new InvalidConsumeBehaviourException(QThrottling.Ignore),
                QThrottling.Reject => throw new InvalidConsumeBehaviourException(QThrottling.Reject),
                QThrottling.BlockWithTimeOut => q.Consume(Context.ConnectionId, timeOutMs),
                _ => throw new NotImplementedException(),
            };

            if (!consumeResult.IsSucceeded)
                return consumeResult.Item;

            var qItem = consumeResult.Item;
            if (q.Properties.Commit == QAcknowledgement.Automatic && qItem.Options.Commit == QAcknowledgement.Automatic)
            {
                var clientInfo = _qMgr.GetClient(Context.ConnectionId);
                _qMtr.StartCommittedItem(queueName, qItem.Id);
                if (clientInfo != null)
                    qItem.CommitedBy = $"{clientInfo.Origin}.{clientInfo.Name}.{clientInfo.Id}";
                q.Commit(qItem);
                await Clients.Group(GetGroupName(QOrigin.Consumer, queueName)).OnCommitted(new QItemOriginEventArgs(queueName, qItem, QOrigin.Consumer));
                _qMtr.EndCommittedItem(queueName, qItem.Id);
            }
            else if (q.Properties.Commit == QAcknowledgement.SemiAutomatic && qItem.Options.Commit == QAcknowledgement.SemiAutomatic)
            {
                await Clients.Caller.OnWaitingForCommitted(new QItemOriginEventArgs(queueName, qItem, QOrigin.Consumer));
                _qMtr.StartCommittedItem(queueName, qItem.Id);
            }
            else if (q.Properties.Commit == QAcknowledgement.Manual && qItem.Options.Commit == QAcknowledgement.Manual)
            {
                await Clients.Caller.OnWaitingForCommitted(new QItemOriginEventArgs(queueName, qItem, QOrigin.Consumer));
                _qMtr.StartCommittedItem(queueName, qItem.Id);
            }

            return await Task.FromResult(consumeResult.Item);
        }

        public async Task<bool> ProduceAsync(string queueName, QItem data, int timeOutMs = -1)
        {
            if (!_qMgr.Exists(queueName))
                throw new QNotExistsException(queueName);

            var q = _qMgr[queueName];
            var produceResult = q.Produce(Context.ConnectionId, data, timeOutMs);
            if (produceResult.IsSucceeded)
            {
                if (q.Properties.Acknowledge == QAcknowledgement.Automatic && data.Options.Acknowledge == QAcknowledgement.Automatic)
                {
                    await Clients.Caller.OnAcknowledged(new QItemOriginEventArgs(queueName, data, QOrigin.Producer));
                    q.Acknowledge(data);
                }
                else if (q.Properties.Acknowledge == QAcknowledgement.SemiAutomatic && data.Options.Acknowledge == QAcknowledgement.SemiAutomatic)
                    await Clients.Caller.OnWaitingForAcknowledged(new QItemOriginEventArgs(queueName, data, QOrigin.Producer));
                else if (q.Properties.Acknowledge == QAcknowledgement.Manual && data.Options.Acknowledge == QAcknowledgement.Manual)
                    await Clients.Caller.OnWaitingForAcknowledged(new QItemOriginEventArgs(queueName, data, QOrigin.Producer));

                await Clients.Group(GetGroupName(QOrigin.Subscriber, queueName)).OnReceived(new QItemOriginEventArgs(queueName, data, QOrigin.Producer));
                return true;
            }
            else
            {
                switch (produceResult.Throttling)
                {
                    case QThrottling.Ignore:
                        await Clients.Caller.OnItemIgnored(new QThrottlingEventArgs(queueName, data, QOrigin.Producer, QThrottling.Ignore));
                        break;
                    case QThrottling.Reject:
                        await Clients.Caller.OnItemIgnored(new QThrottlingEventArgs(queueName, data, QOrigin.Producer, QThrottling.Reject));
                        break;
                    case QThrottling.Block:
                    case QThrottling.BlockWithTimeOut:
                    default:
                        break;
                }
                return false;
            }
        }

        public async Task AcknowledgeAsync(string queueName, QItem data)
        {
            if (_qMgr.Exists(queueName))
            {
                _qMgr[queueName]?.Acknowledge(data);
                await Clients.Caller.OnAcknowledged(new QItemOriginEventArgs(queueName, data, QOrigin.Producer));
            }
        }

        public async Task CommitAsync(string queueName, QItem data)
        {
            if (_qMgr.Exists(queueName))
            {
                _qMgr[queueName]!.Commit(data);
                await Clients.Group(GetGroupName(QOrigin.Subscriber, queueName)).OnCommitted(new QItemOriginEventArgs(queueName, data, QOrigin.Consumer));
                _qMtr.EndCommittedItem(queueName, data.Id);
            }
        }

        public async Task<bool> BindAsync(string queueName)
        {
            Console.WriteLine($"Bind request from {Context.ConnectionId}...");
            if (_qMgr.Exists(queueName))
            {
                _qMgr[queueName]?.Bind(Context.ConnectionId);
                var clientInfo = _qMgr.GetClient(Context.ConnectionId);
                if (clientInfo != null)
                {
                    clientInfo.QueueName = queueName;
                    _qMgr.SetClient(Context.ConnectionId, clientInfo);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(QOrigin.Producer, queueName));
                Console.WriteLine($"Bind request from {Context.ConnectionId}: Succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine($"Bind request from {Context.ConnectionId}: Failed, queue not exist.");
            }

            return false;
        }

        public async Task<bool> UnbindAsync(string queueName)
        {
            Console.WriteLine($"Unbind request from {Context.ConnectionId}...");
            if (_qMgr.Exists(queueName))
            {
                _qMgr[queueName]?.Unsubscribe(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(QOrigin.Subscriber, queueName));
                Console.WriteLine($"Unbind request from {Context.ConnectionId}: Succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine($"Unbind request from {Context.ConnectionId}: Failed, queue not exist.");
            }

            return false;
        }

        public async Task<bool> SubscribeAsync(string queueName)
        {
            Console.WriteLine($"Subscribe request from {Context.ConnectionId}...");
            if (_qMgr.Exists(queueName))
            {
                _qMgr[queueName]?.Subscribe(Context.ConnectionId);
                var clientInfo = _qMgr.GetClient(Context.ConnectionId);
                if (clientInfo != null)
                {
                    clientInfo.QueueName = queueName;
                    _qMgr.SetClient(Context.ConnectionId, clientInfo);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(QOrigin.Subscriber, queueName));
                Console.WriteLine($"Subscribe request from {Context.ConnectionId}: Succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine($"Subscribe request from {Context.ConnectionId}: Failed, queue not exist.");
            }

            return false;
        }

        public async Task<bool> UnsubscribeAsync(string queueName)
        {
            Console.WriteLine($"Unsubscribe request from {Context.ConnectionId}...");
            if (_qMgr.Exists(queueName))
            {
                _qMgr[queueName]?.Unsubscribe(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(QOrigin.Subscriber, queueName));
                Console.WriteLine($"Unsubscribe request from {Context.ConnectionId}: Succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine($"Unsubscribe request from {Context.ConnectionId}: Failed, queue not exist.");
            }

            return false;
        }

        private static string GetGroupName(QOrigin origin, string queueName)
        {

            return origin switch
            {
                QOrigin.Consumer => $"Subscriber.{queueName}",
                QOrigin.Subscriber => $"Subscriber.{queueName}",
                QOrigin.Producer => $"Producer.{queueName}",
                QOrigin.All => $"{queueName}",
                _ => $"{queueName}"
            };
        }

        public ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}