using SignalMQ.Core.Abstractions;
using SignalMQ.Core.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Storages
{
    public struct QueueRecord : IQueueStorageRecord
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public uint Capacity { get; set; }

        public Behaviour Behaviour { get; set; }

        public Properties Properties { get; set; }
    }
}