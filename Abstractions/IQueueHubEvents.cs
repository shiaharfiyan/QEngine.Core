using SignalMQ.Core.EventArgs;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueHubEvents
    {
        //QProducer Events
        Task OnAcknowledged(MessageEventArgs e);
        Task OnWaitingForAcknowledged(MessageEventArgs e);
        Task OnItemRejected(QueueThrottlingEventArgs e);
        Task OnItemIgnored(QueueThrottlingEventArgs e);

        //QSubsriber Events
        Task OnReceived(MessageEventArgs e);

        //QConsumer Events
        Task OnCommitted(MessageEventArgs e);
        Task OnWaitingForCommitted(MessageEventArgs e);

        Task OnIdentified(string name, ClientType type);
        Task OnAttached(string queueName);
        Task OnDetached(string queueName);
        Task OnQueueBehaviourChanged();
    }
}