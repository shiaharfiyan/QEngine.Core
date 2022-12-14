using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Abstractions
{
    public interface IResult
    {
        bool Success { get; }

        Message Message { get; }
    }
}