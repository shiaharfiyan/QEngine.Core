namespace QEngine.Core.EventArgs
{
    public class QChangedEventArgs
    {
        public QChangedEventArgs(string queueName, Q queue, MethodName method)
        {
            QueueName = queueName;
            Queue = queue;
            Method = method;
        }

        public string QueueName { get; init; }
        public Q Queue { get; init; }
        public MethodName Method { get; init; }
    }
}
