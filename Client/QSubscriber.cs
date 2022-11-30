using Microsoft.AspNetCore.SignalR.Client;
using QEngine.Core.Abstractions;
using QEngine.Core.EventArgs;
using QEngine.Core.Server;
using static QEngine.Core.EventArgs.QEventHandlers;

namespace QEngine.Core.Clients
{
    public class QSubscriber : QWorker
    {
        public event ReceivedEventHandler? Received;

        protected bool IsSubscribed { get; set; } = false;

        public QSubscriber(string brokerHost, int brokerPort, bool ssl = false)
            : base(QOrigin.Subscriber, brokerHost, brokerPort, ssl)
        {
        }

        public QSubscriber(QOrigin type, string brokerHost, int brokerPort, bool ssl = false)
            : base(type, brokerHost, brokerPort, ssl)
        {

        }

        public void Subscribe()
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            _hubConnection.On<QItemOriginEventArgs>(nameof(IQEngineHub.OnReceived), OnReceived);
            Task.Run(async () =>
            {
                IsSubscribed = await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.SubscribeAsync), Channel.QueueName);
            }).Wait(new CancellationToken());
        }

        public async Task SubscribeAsync()
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            _hubConnection.On<QItemOriginEventArgs>(nameof(IQEngineHub.OnReceived), OnReceived);
            var subResult = await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.SubscribeAsync), Channel.QueueName);

            IsSubscribed = subResult;
        }

        public void Unsubscribe()
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            _hubConnection.Remove(nameof(IQEngineHub.OnReceived));

            Task.Run(async () =>
            {
                IsSubscribed = !await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.UnsubscribeAsync), Channel.QueueName);
            }).Wait(new CancellationToken());
        }

        public async Task UnsubscribeAsync()
        {
            if (Channel == null)
                throw new NullReferenceException($"{nameof(Channel)} is null. Set Channel by using Use Method");

            _hubConnection.Remove(nameof(IQEngineHub.OnReceived));
            IsSubscribed = !await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.UnsubscribeAsync), Channel.QueueName); ;
        }

        public override void Use(QChannel channel)
        {
            if (channel != null)
            {
                base.Use(channel);
                Subscribe();
            }
        }

        private void OnReceived(QItemOriginEventArgs e)
        {
            Received?.Invoke(e);
        }

        public override void DoDispose()
        {

        }
    }
}