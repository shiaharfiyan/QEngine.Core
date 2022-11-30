using Lifecare.Helper;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using QEngine.Core.Abstractions;
using QEngine.Core.Server;
using static QEngine.Core.EventArgs.QEventHandlers;

namespace QEngine.Core.Clients
{
    public abstract class QWorker : IDisposable
    {
        protected readonly HubConnection _hubConnection;
        protected CancellationTokenSource _tokenSource;
        protected QOrigin _type;
        protected bool _disposed;
        protected bool _autoEcho;
        protected bool _isAutoEchoRunning;
        protected bool _cancel = false;
        protected bool _done = false;

        protected string _host = "";
        protected int _port = 0;
        protected bool _ssl = false;

        private int _heartBeatInterval = 1000;

        public event ConnectedEventHandler? Connected;
        public event DisconnectedEventHandler? Disconnected;
        public event ReconnectingEventHandler? Reconnecting;
        public event ReconnectedEventHandler? Reconnected;
        public event TimeArrivedEventHandler? TimeArrived;

        public QWorker(QOrigin type, string brokerHost, int brokerPort, bool ssl = false)
        {
            _host = brokerHost;
            _port = brokerPort;
            _ssl = ssl;

            _cancel = false;
            _done = false;

            _tokenSource = new CancellationTokenSource();
            _type = type;
            _hubConnection = new HubConnectionBuilder()
                .AddNewtonsoftJsonProtocol()
                .WithAutomaticReconnect()
                .WithUrl($"{(ssl ? "https" : "http")}://{brokerHost}:{brokerPort}/hubs/broker", options =>
                {
                    options.WebSocketConfiguration = conf =>
                    {
                        conf.RemoteCertificateValidationCallback = (message, cert, chain, errors) => { return true; };
                    };
                    options.HttpMessageHandlerFactory = factory => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                    };
                })
                .Build();

            _ = Connect();

            _hubConnection.On(nameof(IQEngineHub.OnAssigned), OnAssigned);
            _hubConnection.On<DateTime>(nameof(IQEngineHub.OnBrokerTimeReceived), OnBrokerTimeReceived);

            _hubConnection.Closed += OnHubConnectionClosed;
            _hubConnection.Reconnecting += OnHubConnectionReconnecting;
            _hubConnection.Reconnected += OnHubConnectionReconnected;
        }

        protected bool Connect()
        {
            if (_hubConnection.StartAsync(5000, _tokenSource.Token))
            {
                Connected?.Invoke(_hubConnection.ConnectionId ?? "");
                DoSend(() =>
                {
                    var result = false;
                    Task.Run(async () =>
                    {
                        result = await _hubConnection.InvokeAsync<bool>(nameof(QEngineHub.AssignAsync), $"{_type}.{Randomizer.Generate(15)}", _type.ToString());
                    }).Wait(_tokenSource.Token);
                    return true;
                });
                Task.Run(async () =>
                {
                    await RequestBrokerTimeAsync();
                    await EchoAsync();
                });

                return true;
            }

            return false;
        }

        private void OnBrokerTimeReceived(DateTime dateTime)
        {
            TimeArrived?.Invoke(dateTime);
        }

        private Task OnHubConnectionClosed(Exception? arg)
        {
            Disconnected?.Invoke();
            return Task.CompletedTask;
        }

        private Task OnHubConnectionReconnected(string? arg)
        {
            Reconnected?.Invoke(_hubConnection.ConnectionId ?? "", DateTime.Now);
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

        public string? Id => _hubConnection?.ConnectionId;

        public QChannel Channel { get; protected set; } = null!;

        public bool AutoEcho
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

        public int HeartBeatInterval
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

        public bool IsCancelled => _cancel;

        protected void OnAssigned()
        {
            Console.WriteLine($"{_type} with Id '{_hubConnection.ConnectionId}' assigned");
        }

        public void Cancel()
        {
            _cancel = true;
        }

        public async Task CancelAsync()
        {
            Cancel();
            await Task.CompletedTask;
        }

        protected virtual void ListenToCancelRequest()
        {
            Task.Run(async () =>
            {
                _done = false;
                while (true)
                {
                    if (IsCancelled && !_done)
                    {
                        var httpClient = new HttpClient
                        {
                            BaseAddress = new Uri($"{(_ssl ? "https" : "http")}://{_host}:{_port}/qm/")
                        };
                        var response = await httpClient.GetStringAsync($"{_hubConnection.ConnectionId}/{Channel.QueueName}");
                        if (!string.IsNullOrWhiteSpace(response))
                            Console.WriteLine(response);
                        _done = true;
                        break;
                    }

                    await Task.Delay(100);
                }
                Console.WriteLine("Listen completed.");
            });
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
                        if (_hubConnection.State == HubConnectionState.Connected)
                        {
                            _hubConnection.StopAsync();
                            _hubConnection.DisposeAsync();
                        }

                        _hubConnection.Closed -= OnHubConnectionClosed;
                        _hubConnection.Reconnecting -= OnHubConnectionReconnecting;
                        _hubConnection.Reconnected -= OnHubConnectionReconnected;
                    }
                }

                _disposed = true;
            }
        }

        public virtual void Use(QChannel channel)
        {
            Channel = channel;
        }

        public abstract void DoDispose();

        protected bool IsConnected
        {
            get
            {
                return _hubConnection.State == HubConnectionState.Connected;
            }
        }

        protected async Task<bool> DoSendAsync(Func<Task<bool>> task)
        {
            int counter = 0;
            while (!IsConnected)
            {
                if (counter < ConnectionTimeOut)
                {
                    await Task.Delay(1000);
                    counter += 1000;
                }
                else
                {
                    return false;
                }
            }

            var result = await Task.Run(() =>
            {
                return task.Invoke();
            }).WaitAsync(new CancellationToken());
            return result;
        }

        protected bool DoSend(Func<bool> task)
        {
            int counter = 0;
            while (!IsConnected)
            {
                if (counter < ConnectionTimeOut)
                {
                    Task.Delay(1000);
                    counter += 1000;
                }
                else
                {
                    return false;
                }
            }

            return task.Invoke();
        }

        protected async Task EchoAsync()
        {
            while (!_tokenSource.IsCancellationRequested && AutoEcho)
            {
                try
                {
                    await _hubConnection.SendAsync(nameof(QEngineHub.EchoAsync));
                    await Task.Delay(HeartBeatInterval);
                }
                catch when (_tokenSource.IsCancellationRequested)
                {
                    //Ignore Exception
                }
            }
        }

        protected async Task RequestBrokerTimeAsync()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    await _hubConnection.SendAsync(nameof(QEngineHub.GetServerTime));
                    await Task.Delay(HeartBeatInterval);
                }
                catch when (_tokenSource.IsCancellationRequested)
                {
                    //Ignore Exception
                }
            }
        }
    }
}