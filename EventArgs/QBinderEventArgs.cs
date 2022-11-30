namespace QEngine.Core.EventArgs
{
    public class QSubscriberEventArgs
    {
        public QSubscriberEventArgs(string id, int oldCount, int newCount)
        {
            Id = id;
            OldCount = oldCount;
            NewCount = newCount;
        }

        public string Id { get; init; }
        public int OldCount { get; init; }
        public int NewCount { get; internal set; }
    }
}