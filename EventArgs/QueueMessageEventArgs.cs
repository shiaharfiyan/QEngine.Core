using SignalMQ.Core.Abstractions;

namespace SignalMQ.Core.EventArgs
{
    public class QueueMessageEventArgs
    {
        public QueueMessageEventArgs(string queueName, Message message, QueueCollectionChangeTrigger trigger)
        {
            QueueName = queueName;
            Message = message;
            Trigger = trigger;
        }

        public string QueueName { get; init; }
        public Message Message { get; init; }
        public QueueCollectionChangeTrigger Trigger { get; init; }
    }
}