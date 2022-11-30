namespace QEngine.Core.Exceptions
{
    public class QNotExistsException : Exception
    {
        public QNotExistsException(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; init; }

        public override string Message => $"Queue Name '{QueueName}' is not exists";
    }
}
