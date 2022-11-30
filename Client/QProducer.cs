using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using QEngine.Core.Abstractions;
using QEngine.Core.EventArgs;
using QEngine.Core.Server;
using static QEngine.Core.EventArgs.QEventHandlers;

namespace QEngine.Core.Clients
{
    public class QProducer : QWorker
    {
        public event AcknowledgedEventHandler? Acknowledged;
        public event WaitingForAcknowledgedEventHandler? WaitingForAcknowledged;
        public event QueueThrottlingEventHandler? ItemRejected;
        public event QueueThrottlingEventHandler? ItemIgnored;

        public QProducer(string brokerHost, int brokerPort, bool ssl = false)
            : base(QOrigin.Producer, brokerHost, brokerPort, ssl)
        {
            _hubConnection.On<QItemOriginEventArgs>(nameof(IQEngineHub.OnAcknowledged), OnAcknowledged);
            _hubConnection.On<QItemOriginEventArgs>(nameof(IQEngineHub.OnWaitingForAcknowledged), OnWaitingForAcknowledged);

            _hubConnection.On<QThrottlingEventArgs>(nameof(IQEngineHub.OnItemRejected), OnItemRejected);
            _hubConnection.On<QThrottlingEventArgs>(nameof(IQEngineHub.OnItemIgnored), OnItemIgnored);
        }

        private void OnAcknowledged(QItemOriginEventArgs e)
        {
            Acknowledged?.Invoke(e);
        }

        private async void OnWaitingForAcknowledged(QItemOriginEventArgs e)
        {
            switch (Channel.Acknowledge)
            {
                case QAcknowledgement.Automatic:
                    break;
                case QAcknowledgement.SemiAutomatic:
                    Console.WriteLine("Acknowledge triggered automatically from client side");
                    await DoSendAsync(async () =>
                    {
                        await _hubConnection.SendAsync(nameof(QEngineHub.AcknowledgeAsync), Channel.QueueName, e.Value);
                        return true;
                    });
                    break;
                case QAcknowledgement.Manual:
                    WaitingForAcknowledged?.Invoke(e);
                    break;
                default:
                    break;
            }
        }

        private void OnItemRejected(QThrottlingEventArgs e)
        {
            ItemRejected?.Invoke(e);
        }

        private void OnItemIgnored(QThrottlingEventArgs e)
        {
            ItemIgnored?.Invoke(e);
        }

        public async Task<bool> ProduceAsync(object data, int timeOutMs = -1)
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            var qItem = Channel.Create(data);

            if (!IsConnected)
            {
                Console.WriteLine("Client not connected.");
                var reconnectResult = Connect();
                if (reconnectResult)
                    Use(Channel);
            }

            return await DoSendAsync(async () =>
            {
                try
                {
                    ListenToCancelRequest();
                    var result = await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.ProduceAsync), Channel.QueueName, qItem, timeOutMs);
                    _done = true;
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while produce: {ex.Message}");
                    return false;
                }
            });
        }

        public async Task AcknowledgeAsync(QItem data)
        {
            data.IsAcknowledged = true;

            await DoSendAsync(async () =>
            {
                await _hubConnection.SendAsync(nameof(QEngineHub.AcknowledgeAsync), Channel.QueueName, data);
                return true;
            });
        }

        public bool IsBinded { get; set; }

        public override void Use(QChannel channel)
        {
            if (channel != null)
            {
                base.Use(channel);
                Bind();
            }
        }

        public async void Bind()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
                IsBinded = await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.BindAsync), Channel.QueueName);
        }

        public async void Unbind()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
                IsBinded = !(await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.UnbindAsync), Channel.QueueName));
        }

        public override void DoDispose()
        {

        }
    }
}