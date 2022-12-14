using Microsoft.AspNetCore.SignalR.Client;
using SignalMQ.Core.Abstractions;
using SignalMQ.Core.EventArgs;
using System.Net;
using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Clients
{
    public abstract class QConsumer : QWorker
    {
        public event WaitingForCommittedEventHandler? WaitingForCommitted;
        public event CommittedEventHandler? Committed;

        public QConsumer(string name, IChannel channel)
            : base(ClientType.Consumer, name, channel) { }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();

            if (_hubConnection != null)
            {
                _hubConnection.On<CommittedEventArgs>(nameof(IQueueHubEvents.OnCommitted), OnCommitted);
                _hubConnection.On<MessageEventArgs>(nameof(IQueueHubEvents.OnWaitingForCommitted), OnWaitingForCommitted);
            }
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();

            if (_hubConnection != null)
            {
                _hubConnection.Remove(nameof(IQueueHubEvents.OnCommitted));
                _hubConnection.Remove(nameof(IQueueHubEvents.OnWaitingForCommitted));
            }
        }

        private void OnCommitted(CommittedEventArgs e)
        {
            Committed?.Invoke(e);
        }

        private async void OnWaitingForCommitted(MessageEventArgs e)
        {
            switch (Channel.Commit)
            {
                case Acknowledgement.AutoServerSide:
                    break;
                case Acknowledgement.AutoClientSide:
                    Console.WriteLine("Commit triggered automatically from client side");
                    await this.DoSendAsync(async () =>
                    {
                        await _hubConnection.SendAsync(nameof(IQueueHub.CommitAsync), e.Message);
                        return true;
                    });
                    break;
                case Acknowledgement.Manual:
                    WaitingForCommitted?.Invoke(e);
                    break;
                default:
                    break;
            }
        }

        public async Task CommitAsync(string id, string committedBy = "")
        {
            string committedByMessage = string.IsNullOrWhiteSpace(committedBy) ?
                    $"{Dns.GetHostName()}.{Name}.{ConnectionId}" :
                    committedBy;

            await _hubConnection.SendAsync(nameof(IQueueHub.CommitAsync), id, committedByMessage);
        }

        protected override void DoDispose()
        {

        }
    }
}
