namespace QEngine.Core.Monitoring
{
    public struct QMonitorItem
    {
        public QMonitorItem(string id, string queueName)
        {
            Id = id;
            QueueName = queueName;
        }

        public string Id { get; set; }
        public string QueueName { get; set; }
        public bool IsFinished { get; set; } = false;

        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime End { get; set; } = DateTime.MinValue;
        public double Duration
        {
            get
            {
                if (!IsFinished)
                    return -1;

                return (End - Start).TotalMilliseconds;
            }
        }
    }
}
