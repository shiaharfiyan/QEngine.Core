namespace QEngine.Core.EventArgs
{
    public class QThrottlingEventArgs
    {
        public QThrottlingEventArgs(string queueName, QItem value, QOrigin origin, QThrottling throttling)
        {
            QueueName = queueName;
            Value = value;
            Origin = origin;
            Throttling = throttling;
        }

        public string QueueName { get; init; }
        public QItem Value { get; init; }
        public QOrigin Origin { get; init; }
        public QThrottling Throttling { get; init; }
    }
}
