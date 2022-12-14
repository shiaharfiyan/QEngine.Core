using SignalMQ.Core.Results;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueHub : IAsyncDisposable
    {
        Task<DateTime> GetServerTime();
        Task HealthCheckAsync();

        Task<CallResult> ConsumeAsync(int timeOutMs = -1);
        Task<CallResult> ProduceAsync(Message data, int timeOutMs = -1);

        Task AcknowledgeAsync(Message data);
        Task CommitAsync(Message data, string committedBy = "");

        Task<CallResult> IdentifyAsync(string name, ClientType type);
        Task<CallResult> AttachAsync(string queueName);
        Task<CallResult> DetachAsync();
    }
}