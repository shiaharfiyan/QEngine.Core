using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Abstractions
{
    public interface IProducer<T>
    {
        Task<bool> ProduceAsync(T data, int timeOutMs = -1, CallBackDelegate? callBack = null);
        Task<bool> ProduceAsync(Message<T> data, int timeOutMs = -1, CallBackDelegate? callBack = null);
    }
}