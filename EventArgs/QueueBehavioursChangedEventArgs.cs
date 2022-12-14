namespace SignalMQ.Core.EventArgs
{
    public class QueueBehavioursChangedEventArgs
    {
        public QueueBehavioursChangedEventArgs(string queueName, QueueChangeTrigger trigger)
        {
            QueueName = queueName;
            Trigger = trigger;
            Changes = new Dictionary<string, (object, object)>();
        }

        public string QueueName { get; init; }
        public Dictionary<string, (object, object)> Changes { get; init; }
        public QueueChangeTrigger Trigger { get; init; }
    }
}
