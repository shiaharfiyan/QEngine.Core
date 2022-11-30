namespace QEngine.Core.EventArgs
{
    public class QItemEventArgs
    {
        public QItemEventArgs(string queueName, QItem value, QMethodName method)
        {
            QueueName = queueName;
            Value = value;
            Method = method;
        }

        public string QueueName { get; init; }
        public QItem Value { get; init; }
        public QMethodName Method { get; init; }
    }
}
