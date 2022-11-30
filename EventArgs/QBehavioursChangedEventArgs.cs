namespace QEngine.Core.EventArgs
{
    public class QBehavioursChangedEventArgs
    {
        public QBehavioursChangedEventArgs(string queueName, MethodName method)
        {
            QueueName = queueName;
            Method = method;
            Changes = new Dictionary<string, (object, object)>();
        }

        public string QueueName { get; init; }
        public Dictionary<string, (object, object)> Changes { get; init; }
        public MethodName Method { get; init; }
    }
}
