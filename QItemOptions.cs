namespace QEngine.Core
{
    public struct QItemOptions
    {
        public bool Persistent { get; set; }
        public bool AutoDelete { get; set; }

        public QAcknowledgement Acknowledge { get; set; }
        public QAcknowledgement Commit { get; set; }
    }
}
