using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core
{
    public struct Properties
    {
        public Properties()
        {

        }

        /// <summary>
        /// Store data to persistent storage if set to true, otherwise false
        /// </summary>
        public bool Persistent { get; set; } = true;

        /// <summary>
        /// Acknowledge data produced to queue
        /// This properties can be overriden by message properties
        /// If both Acknowledge on message properties & queue properties set to None, AutoServerSide will be triggered
        /// </summary>
        public Acknowledgement Acknowledge { get; set; } = Acknowledgement.AutoServerSide;

        /// <summary>
        /// Commit data consumed from queue
        /// This properties can be overriden by message properties
        /// If both Commit on message properties & queue properties set to None, AutoClientSide will be triggered
        /// </summary>
        public Acknowledgement Commit { get; set; } = Acknowledgement.AutoClientSide;

        public override string ToString()
        {
            var contentList = new List<string>
            {
                $"{(Persistent ? "Persistent" : "")}",
                $"{Acknowledge}",
                $"{Commit}"
            };
            return string.Join(",", contentList.ToArray());
        }
    }
}
