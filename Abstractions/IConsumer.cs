using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Abstractions
{
    public interface IConsumer<T>
    {
        event CommittedEventHandler<T>? Committed;
        event WaitingForCommittedEventHandler<T>? WaitingForCommitted;

        Task<Message<T>> ConsumeAsync(int timeOutMs = -1, CallBackDelegate? callBack = null);
        Task CommitAsync(Message<T> data, string committedBy = "");
    }
}