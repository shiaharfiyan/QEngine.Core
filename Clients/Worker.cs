using Microsoft.AspNetCore.SignalR.Client;
using SignalMQ.Core.Abstractions;
using SignalMQ.Core.Extensions;
using SignalMQ.Core.Results;
using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Clients
{
    public abstract class Worker : IWorker, IDisposable
    {
        protected CancellationTokenSource _tokenSource;

        protected bool _disposed;
        protected bool _autoEcho;
        protected bool _isAutoEchoRunning;

        protected bool _isRequestDone;

        private int _heartBeatInterval = 1000;

        public event ConnectedEventHandler? Connected;
        public event DisconnectedEventHandler? Disconnected;
        public event ReconnectingEventHandler? Reconnecting;
        public event ReconnectedEventHandler? Reconnected;

        protected HubConnection _hubConnection;

        public Worker(ClientType type, string name, IChannel channel)
        {
            Name = name;
            Type = type;
            _tokenSource = new CancellationTokenSource();

            Channel = channel ?? throw new ArgumentNullException(nameof(channel));

            _hubConnection = Channel.BuildHub();

            _ = Connect();
        }

        public string Name { get; init; }

        public ClientType Type { get; init; }

        public string? ConnectionId => _hubConnection?.ConnectionId;

        public bool EnableHealthCheck
        {
            get => _autoEcho;
            set
            {
                if (_autoEcho)
                {
                    if (!value)
                    {
                        _tokenSource.Cancel();
                        _tokenSource.Dispose();
                        _tokenSource = new CancellationTokenSource();

                        _isAutoEchoRunning = false;
                        _autoEcho = value;
                    }
                }
                else
                {
                    if (value)
                    {
                        _isAutoEchoRunning = true;
                        _autoEcho = value;
                    }
                }
            }
        }

        public int ConnectionTimeOut { get; set; } = 120000;

        public int HealthCheckInterval
        {
            get
            {
                return _heartBeatInterval;
            }
            set
            {
                if (value >= 1000)
                    _heartBeatInterval = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _hubConnection?.State == HubConnectionState.Connected;
            }
        }

        public bool IsAttached { get; set; }

        private Task OnHubConnectionClosed(Exception? arg)
        {
            Disconnected?.Invoke();
            return Task.CompletedTask;
        }

        private Task OnHubConnectionReconnected(string? arg)
        {
            Reconnected?.Invoke(_hubConnection?.ConnectionId ?? "", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnHubConnectionReconnecting(Exception? arg)
        {
            int counter = 0;
            while (!IsConnected)
            {
                Reconnecting?.Invoke(counter / 1000, ConnectionTimeOut / 1000);
                Task.Delay(1000);
            }
            return Task.CompletedTask;
        }

        protected virtual void RegisterEvents()
        {
            _hubConnection?.On<string, ClientType>(nameof(IQueueHubEvents.OnIdentified), OnIdentified);
            _hubConnection?.On<string>(nameof(IQueueHubEvents.OnAttached), OnAttached);
            _hubConnection?.On<string>(nameof(IQueueHubEvents.OnDetached), OnDetached);

            if(_hubConnection != null)
            {
                _hubConnection.Closed += OnHubConnectionClosed;
                _hubConnection.Reconnecting += OnHubConnectionReconnecting;
                _hubConnection.Reconnected += OnHubConnectionReconnected;
            }
        }

        protected virtual void UnregisterEvents()
        {
            _hubConnection?.Remove(nameof(IQueueHubEvents.OnIdentified));
            _hubConnection?.Remove(nameof(IQueueHubEvents.OnAttached));

            if (_hubConnection != null)
            {
                _hubConnection.Closed -= OnHubConnectionClosed;
                _hubConnection.Reconnecting -= OnHubConnectionReconnecting;
                _hubConnection.Reconnected -= OnHubConnectionReconnected;
            }
        }

        protected virtual bool Connect()
        {
            if (_hubConnection.StartAsync(5000, _tokenSource.Token))
            {
                Connected?.Invoke(_hubConnection.ConnectionId ?? "");
                RegisterEvents();

                Task.Run(async () =>
                {
                    await IdentifyAsync((cr) => { });
                }).Wait();

                Task.Run(async () =>
                {
                    await AttachAsync((cr) => { });
                }).Wait();

                Task.Run(SendHealthCheckAsync).Wait();

                _isRequestDone = false;

                return true;
            }

            return false;
        }

        protected void OnIdentified(string name, ClientType type)
        {
            Console.WriteLine($"{Name}({Type}) with Id '{_hubConnection?.ConnectionId}' identified.");
        }

        protected void OnAttached(string queueName)
        {
            Console.WriteLine($"{Name}({Type}) attached to {queueName}");
            IsAttached = true;
        }

        protected void OnDetached(string queueName)
        {
            Console.WriteLine($"{Name}({Type}) attached to {queueName}");
            IsAttached = false;
        }

        public IChannel Channel { get; }

        public bool ThrowIfException { get; set; }

        public async Task CancelAsync()
        {
            if (!_isRequestDone)
            {
                var httpClient = new HttpClient { BaseAddress = Channel.BaseUri };
                var response = await httpClient.GetStringAsync($"qm/{_hubConnection?.ConnectionId}/{Channel.QueueName}");
                if (!string.IsNullOrWhiteSpace(response))
                    Console.WriteLine(response);

                _isRequestDone = true;
            }
        }

        public void Dispose()
        {
            DoDispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _tokenSource.Cancel();

                    if (_hubConnection != null)
                    {
                        UnregisterEvents();

                        if (_hubConnection.State == HubConnectionState.Connected)
                            _hubConnection.StopAsync();

                        _hubConnection.DisposeAsync();
                    }
                }

                _disposed = true;
            }
        }

        protected async Task SendHealthCheckAsync()
        {
            while (!_tokenSource.IsCancellationRequested && EnableHealthCheck)
            {
                try
                {
                    await _hubConnection.SendAsync(nameof(IQueueHub.HealthCheckAsync));
                    await Task.Delay(HealthCheckInterval);
                }
                catch when (_tokenSource.IsCancellationRequested)
                {
                    //Ignore Exception
                }
            }
        }

        protected virtual async Task IdentifyAsync(CallBackDelegate callBack)
        {
            var callResult = await _hubConnection.InvokeAsync<CallResult>(nameof(IQueueHub.IdentifyAsync), Name, Type);
            callBack?.Invoke(new CallBackResult(callResult.Success, callResult.Value()));
        }

        protected virtual async Task AttachAsync(CallBackDelegate callBack)
        {
            var callResult = await _hubConnection.InvokeAsync<CallResult>(nameof(IQueueHub.AttachAsync), Channel.QueueName);
            IsAttached = callResult.Success;

            callBack?.Invoke(new CallBackResult(callResult.Success, callResult.Value()));
        }

        protected virtual async Task DetachAsync(CallBackDelegate callBack)
        {
            var callResult = await _hubConnection.InvokeAsync<CallResult>(nameof(IQueueHub.DetachAsync));
            IsAttached = !callResult.Success;

            callBack?.Invoke(new CallBackResult(callResult.Success, callResult.Value()));
        }

        protected abstract void DoDispose();

        public async Task<DateTime> GetServerTimeAsync()
        {
            return await _hubConnection.InvokeAsync<DateTime>(nameof(IQueueHub.GetServerTime));
        }
    }
}