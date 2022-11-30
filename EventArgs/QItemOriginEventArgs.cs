namespace QEngine.Core.EventArgs
{
    public class QItemOriginEventArgs
    {
        public QItemOriginEventArgs(string queueName, QItem value, QOrigin origin)
        {
            QueueName = queueName;
            Value = value;
            Origin = origin;
        }

        public string QueueName { get; init; }
        public QItem Value { get; init; }
        public QOrigin Origin { get; init; }
    }
}
