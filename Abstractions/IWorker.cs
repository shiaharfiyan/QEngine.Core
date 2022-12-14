namespace SignalMQ.Core.Abstractions
{
    public interface IWorker
    {
        string Name { get; }
        ClientType Type { get; }

        bool IsConnected { get; }
        bool IsAttached { get; }

        string? ConnectionId { get; }
        int ConnectionTimeOut { get; }
        bool EnableHealthCheck { get; set; }
        int HealthCheckInterval { get; set; }

        bool ThrowIfException { get; set; }

        IChannel Channel { get; }
    }
}
