using SignalMQ.Core.Abstractions;

namespace SignalMQ.Core.EventArgs
{
    public class MessageEventArgs
    {
        public MessageEventArgs(string queueName, Message message)
        {
            QueueName = queueName;
            Message = message;
        }

        public string QueueName { get; init; }

        public Message Message { get; init; }
    }

    public class MessageEventArgs<T>
    {
        public MessageEventArgs(string queueName, Message<T> message)
        {
            QueueName = queueName;
            Message = message;
        }

        public string QueueName { get; init; }

        public Message<T> Message { get; init; }
    }
}