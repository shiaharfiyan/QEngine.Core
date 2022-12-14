using Microsoft.AspNetCore.SignalR.Client;
using SignalMQ.Core.Abstractions;
using SignalMQ.Core.EventArgs;
using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Clients
{
    public abstract class QProducer : QWorker
    {
        public event AcknowledgedEventHandler? Acknowledged;
        public event WaitingForAcknowledgedEventHandler? WaitingForAcknowledged;
        public event QueueThrottlingEventHandler? ItemRejected;
        public event QueueThrottlingEventHandler? ItemIgnored;

        public QProducer(string name, IChannel channel)
            : base(ClientType.Producer, name, channel) { }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();

            if (_hubConnection != null)
            {
                _hubConnection.On<AcknowledgedEventArgs>(nameof(IQueueHubEvents.OnAcknowledged), OnAcknowledged);
                _hubConnection.On<MessageEventArgs>(nameof(IQueueHubEvents.OnWaitingForAcknowledged), OnWaitingForAcknowledged);
                _hubConnection.On<QueueThrottlingEventArgs>(nameof(IQueueHubEvents.OnItemRejected), OnItemRejected);
                _hubConnection.On<QueueThrottlingEventArgs>(nameof(IQueueHubEvents.OnItemIgnored), OnItemIgnored);
            }
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();

            if (_hubConnection != null)
            {
                _hubConnection.Remove(nameof(IQueueHubEvents.OnAcknowledged));
                _hubConnection.Remove(nameof(IQueueHubEvents.OnWaitingForAcknowledged));
                _hubConnection.Remove(nameof(IQueueHubEvents.OnItemRejected));
                _hubConnection.Remove(nameof(IQueueHubEvents.OnItemIgnored));
            }
        }

        private void OnAcknowledged(AcknowledgedEventArgs e)
        {
            Acknowledged?.Invoke(e);
        }

        private async void OnWaitingForAcknowledged(MessageEventArgs e)
        {
            switch (Channel.Acknowledge)
            {
                case Acknowledgement.AutoServerSide:
                    break;
                case Acknowledgement.AutoClientSide:
                    Console.WriteLine("Acknowledge triggered automatically from client side");
                    await this.DoSendAsync(async () =>
                    {
                        await _hubConnection.SendAsync(nameof(IQueueHub.AcknowledgeAsync), e.Message);
                        return true;
                    });
                    break;
                case Acknowledgement.Manual:
                    WaitingForAcknowledged?.Invoke(e);
                    break;
                default:
                    break;
            }
        }

        private void OnItemRejected(QueueThrottlingEventArgs e)
        {
            ItemRejected?.Invoke(e);
        }

        private void OnItemIgnored(QueueThrottlingEventArgs e)
        {
            ItemIgnored?.Invoke(e);
        }

        public async Task AcknowledgeAsync(string id)
        {
            await this.DoSendAsync(async () =>
            {
                await _hubConnection.SendAsync(nameof(IQueueHub.AcknowledgeAsync), id);
                return true;
            });
        }

        protected override void DoDispose()
        {

        }
    }
}
