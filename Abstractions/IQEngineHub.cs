using QEngine.Core.EventArgs;

namespace QEngine.Core.Abstractions
{
    public interface IQEngineHub
    {
        //QProducer Events
        Task OnAcknowledged(QItemOriginEventArgs e);
        Task OnWaitingForAcknowledged(QItemOriginEventArgs e);
        Task OnItemRejected(QThrottlingEventArgs e);
        Task OnItemIgnored(QThrottlingEventArgs e);

        //QConsumer Events
        Task OnReceived(QItemOriginEventArgs e);
        Task OnCommitted(QItemOriginEventArgs e);
        Task OnWaitingForCommitted(QItemOriginEventArgs e);

        Task OnBrokerTimeReceived(DateTime dateTime);
        Task OnAssigned();
        Task OnQueueBehaviourChanged();
    }
}