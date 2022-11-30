namespace QEngine.Core.Exceptions
{
    public class NotSubscribedException : Exception
    {
        public NotSubscribedException(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; init; }

        public override string Message => $"Not subscribed to Queue Name '{QueueName}' yet";
    }
}
