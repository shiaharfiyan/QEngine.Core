namespace SignalMQ.Core.Exceptions
{
    public class QueueNotExistsException : Exception
    {
        public QueueNotExistsException(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; init; }

        public override string Message => $"Queue Name '{QueueName}' is not exists";
    }
}
