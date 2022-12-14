using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueMessageStorageRecord
    {
        string Id { get; set; }
        string ValueAsJson { get; set; }

        bool IsCommitted { get; set; }
        bool IsAcknowledged { get; set; }
        string CommitedBy { get; set; }

        DateTime ProducedTime { get; set; }
        DateTime? AcknowledgedTime { get; set; }
        DateTime? CommittedTime { get; set; }

        Properties Properties { get; set; }
    }
}
