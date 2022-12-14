using SignalMQ.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Results
{
    public struct AbortResult : IResult
    {
        public AbortResult(bool success, Message value)
        {
            Success = success;
            Message = value;
        }

        public bool Success { get; set; } = true;

        public Message Message { get; set; }
    }
}
