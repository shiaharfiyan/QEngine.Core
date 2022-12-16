using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Abstractions
{
    public interface IProducer<T>
    {
        event AcknowledgedEventHandler<T>? Acknowledged;
        event WaitingForAcknowledgedEventHandler<T>? WaitingForAcknowledged;

        Task<bool> ProduceAsync(T data, int timeOutMs = -1, CallBackDelegate? callBack = null);
        Task<bool> ProduceAsync(Message<T> message, int timeOutMs = -1, CallBackDelegate? callBack = null);
        Task AcknowledgeAsync(Message<T> message);
    }
}