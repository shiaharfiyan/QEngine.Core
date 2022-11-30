namespace QEngine.Core.EventArgs
{
    public class QClientEventArgs
    {
        public QClientEventArgs(string id, QClientInfo client, int oldCount, int newCount)
        {
            Id = id;
            Client = client;
            OldCount = oldCount;
            NewCount = newCount;
        }

        public string Id { get; init; }
        public QClientInfo Client { get; set; }
        public int OldCount { get; init; }
        public int NewCount { get; internal set; }
    }
}
