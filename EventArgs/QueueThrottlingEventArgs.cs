using SignalMQ.Core.Abstractions;

namespace SignalMQ.Core.EventArgs
{
    public class QueueThrottlingEventArgs
    {
        public QueueThrottlingEventArgs(string queueName, Message value, ClientType origin, Throttling throttling)
        {
            QueueName = queueName;
            Value = value;
            Origin = origin;
            Throttling = throttling;
        }

        public string QueueName { get; init; }
        public Message Value { get; init; }
        public ClientType Origin { get; init; }
        public Throttling Throttling { get; init; }
    }
}
