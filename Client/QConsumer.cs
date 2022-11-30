using Microsoft.AspNetCore.SignalR.Client;
using QEngine.Core.Abstractions;
using QEngine.Core.EventArgs;
using QEngine.Core.Exceptions;
using QEngine.Core.Server;
using System.Net;
using static QEngine.Core.EventArgs.QEventHandlers;

namespace QEngine.Core.Clients
{
    public class QConsumer : QSubscriber
    {
        public event WaitingForCommittedEventHandler? WaitingForCommitted;
        public event CommittedEventHandler? Committed;

        public QConsumer(string brokerHost, int brokerPort, bool ssl = false)
            : base(QOrigin.Consumer, brokerHost, brokerPort, ssl)
        {
            _hubConnection.On<QItemOriginEventArgs>(nameof(IQEngineHub.OnCommitted), OnCommitted);
            _hubConnection.On<QItemOriginEventArgs>(nameof(IQEngineHub.OnWaitingForCommitted), OnWaitingForCommitted);
        }

        private void OnCommitted(QItemOriginEventArgs e)
        {
            Committed?.Invoke(e);
        }

        private async void OnWaitingForCommitted(QItemOriginEventArgs e)
        {
            switch (Channel.Commit)
            {
                case QAcknowledgement.Automatic:
                    break;
                case QAcknowledgement.SemiAutomatic:
                    Console.WriteLine("Commit triggered automatically from client side");
                    await _hubConnection.SendAsync(nameof(QEngineHub.CommitAsync), Channel.QueueName, e.Value);
                    break;
                case QAcknowledgement.Manual:
                    WaitingForCommitted?.Invoke(e);
                    break;
                default:
                    break;
            }
        }

        public async Task<T> ConsumeAsync<T>(int timeOutMs = -1)
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            if (string.IsNullOrWhiteSpace(Channel.QueueName))
                throw new NullReferenceException($"{nameof(Channel.QueueName)} can not be empty or null");

            if (!IsSubscribed)
                throw new NotSubscribedException(Channel.QueueName);

            QItem result = new();

            if (!IsConnected)
            {
                Console.WriteLine("Client not connected.");
                var reconnectResult = Connect();
                if (reconnectResult)
                    Use(Channel);
            }

            try
            {
                ListenToCancelRequest();
                result = await _hubConnection.InvokeAsync<QItem>(nameof(QEngineHub.ConsumeAsync), Channel.QueueName, timeOutMs);
                _done = true;
                if (result.IsTimeOut)
                    return default!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while consume: {ex.Message}");
                return default!;
            }

            if (!result.IsNullOrEmpty)
            {
                return (T)result.Value;
            }
            else
                return default!;
        }

        public async Task<QItem> ConsumeAsync(int timeOutMs = -1)
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            if (string.IsNullOrWhiteSpace(Channel.QueueName))
                throw new NullReferenceException($"{nameof(Channel.QueueName)} can not be empty or null");

            if (!IsSubscribed)
                throw new NotSubscribedException(Channel.QueueName);

            if (!IsConnected)
            {
                Console.WriteLine("Client not connected.");
                var reconnectResult = Connect();
                if (reconnectResult)
                    Use(Channel);
            }

            try
            {
                ListenToCancelRequest();
                var result = await _hubConnection.InvokeAsync<QItem>(nameof(QEngineHub.ConsumeAsync), Channel.QueueName, timeOutMs);
                _done = true;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while consume: {ex.Message}");
                return default!;
            }
        }

        public async Task CommitAsync(QItem data, string committedBy = "")
        {
            data.IsCommitted = true;
            if (string.IsNullOrWhiteSpace(committedBy))
            {
                data.CommitedBy = $"{Dns.GetHostName()}.{Channel.Name}.{Id}";
            }
            else
            {
                data.CommitedBy = committedBy;
            }
            await _hubConnection.SendAsync(nameof(QEngineHub.CommitAsync), Channel.QueueName, data);
        }
    }
}