﻿using SignalMQ.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core
{
    public struct Behaviour
    {
        public Behaviour() { }

        private Throttling _consumeBehaviour;

        /// <summary>
        /// The behaviour when add the item to queue. 
        /// Block will try to add item and wait indefinitely until space available.
        /// BlockWithTimeOut will try to add item and wait until TimeOut, see ProduceTimeOut property.
        /// Ignore will try to add item with 0 TimeOut, Ignore if failed and raise ItemIgnored events.
        /// Reject will try to add item with 0 TimeOut, Reject if failed and raise ItemRejected events.
        /// </summary>
        public Throttling ProduceBehaviour { get; set; }
        /// <summary>
        /// The number of milliseconds to wait for the collection to add the item, or Infinite (-1) to wait indefinitely.
        /// If Produce Behaviour set to Block, this TimeOut setting will be ignored
        /// </summary>
        public int ProduceTimeOut { get; set; } = -1;

        /// <summary>
        /// The behaviour when take the item from queue.
        /// Only Block/BlockWithTimeOut will be used for ConsumeBehaviour.
        /// Block will try to take item and wait indefinitely until the item available.
        /// BlockWithTimeOut will try to take item and wait until TimeOut, see ConsumeTimeOut property.
        /// </summary>
        public Throttling ConsumeBehaviour
        {
            get { return _consumeBehaviour; }
            set
            {
                switch (value)
                {
                    case Throttling.Block:
                    case Throttling.BlockWithTimeOut:
                        _consumeBehaviour = value;
                        break;
                    case Throttling.Ignore:
                    case Throttling.Reject:
                    default:
                        throw new InvalidConsumeBehaviourException(value);
                }
            }
        }
        /// <summary>
        /// The number of milliseconds to wait for the collection to take the item, or Infinite (-1) to wait indefinitely.
        /// If Consume Behaviour set to Block, this TimeOut setting will be ignored
        /// </summary>
        public int ConsumeTimeOut { get; set; } = -1;
    }
}
