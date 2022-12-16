using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.EventArgs
{
    public class AcknowledgedEventArgs
    {
        public AcknowledgedEventArgs(string queueName, Message message, DateTime acknowledgedDate)
        {
            QueueName = queueName;
            Message = message;
            AcknowledgedDate = acknowledgedDate;
        }

        public string QueueName { get; init; }

        public Message Message { get; init; }

        public DateTime AcknowledgedDate { get; init; }
    }

    public class AcknowledgedEventArgs<T>
    {
        public AcknowledgedEventArgs(string queueName, Message<T> message, DateTime acknowledgedDate)
        {
            QueueName = queueName;
            Message = message;
            AcknowledgedDate = acknowledgedDate;
        }

        public string QueueName { get; init; }

        public Message<T> Message { get; init; }

        public DateTime AcknowledgedDate { get; init; }
    }
}