using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueue : IQueueStorageRecord
    {        
        /// <summary>
        /// Triggered when collection of item has changed
        /// </summary>
        event QueueItemChangedEventHandler? ItemChanged;
        /// <summary>
        /// Triggered when collection of item is full or at max (capacity) and inform producers that item is rejected
        /// </summary>
        event QueueThrottlingEventHandler? ItemRejected;
        /// <summary>
        /// Triggered when collection of item is full or at max (capacity) and inform producers that item is ignored
        /// </summary>
        event QueueThrottlingEventHandler? ItemIgnored;
        /// <summary>
        /// Triggered when collection of producer & subscriber has changed
        /// </summary>
        event AttachersChangedEventHandler? AttachersChanged;

        int MessageCount { get; }

        IResult Produce(Message data, int timeOutMs = -1, CancellationTokenSource? tokenSource = null);

        IResult Consume(int timeOutMs = -1, CancellationTokenSource? tokenSource = null);
    }
}