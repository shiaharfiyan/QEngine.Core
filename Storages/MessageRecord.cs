using SignalMQ.Core.Abstractions;
using SignalMQ.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Storages
{
    public struct MessageRecord : IQueueMessageStorageRecord
    {
        public MessageRecord(string json, string type) : this()
        {
            Id = Randomizer.Generate(32);
            Properties = new Properties();
            ProducedTime = DateTime.Now;
            ValueAsJson = json;
            ValueType = type;
        }

        public string Id { get; set; }

        public string ValueAsJson { get; set; }

        public string ValueType { get; set; }

        public bool IsCommitted { get; set; } = false;

        public bool IsAcknowledged { get; set; } = false;

        public string CommitedBy { get; set; } = string.Empty;

        public DateTime ProducedTime { get; set; }

        public DateTime? AcknowledgedTime { get; set; } = null;

        public DateTime? CommittedTime { get; set; } = null;

        public Properties Properties { get; set; }
    }
}