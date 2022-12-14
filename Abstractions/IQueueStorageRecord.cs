using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueStorageRecord
    {
        string Id { get; set; }

        string Name { get; set; }

        uint Capacity { get; set; }

        Behaviour Behaviour { get; set; }

        Properties Properties { get; set; }
    }
}