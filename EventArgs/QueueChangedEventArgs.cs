namespace SignalMQ.Core.EventArgs
{
    public class QueueChangedEventArgs
    {
        public QueueChangedEventArgs(string queueName, QueueChangeTrigger trigger)
        {
            QueueName = queueName;
            Trigger = trigger;
        }

        public string QueueName { get; init; }
        public QueueChangeTrigger Trigger { get; init; }
    }
}