namespace QEngine.Core.Abstractions
{
    public interface IStorage
    {
        string Name { get; }
        string Connection { get; set; }

        IEnumerable<QItem> Items { get; }

        bool Add(QItem o);
        QItem Remove(string id);
        QItem Get(string id);
        bool Acknowledge(string id);
        bool Commit(string id, string commitedBy);
        int Commit();
    }
}
