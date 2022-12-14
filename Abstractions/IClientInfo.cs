using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Abstractions
{
    public interface IClientInfo
    {
        string ConnectionId { get; }
        string Name { get; }
        string Address { get; }
        string QueueName { get; set; }
        ClientType Type { get; }
        DateTime LastHealthCheck { get; set; }
    }
}
