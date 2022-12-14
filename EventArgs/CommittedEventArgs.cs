using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.EventArgs
{
    public class CommittedEventArgs
    {
        public CommittedEventArgs(string queueName, Message message, 
            DateTime committedDate, string committedBy)
        {
            QueueName = queueName;
            Message = message;
            CommittedBy = committedBy;
            CommittedDate = committedDate;
        }

        public string QueueName { get; init; }

        public Message Message { get; init; }

        public DateTime CommittedDate { get; init; }

        public string CommittedBy { get; init; }
    }
}