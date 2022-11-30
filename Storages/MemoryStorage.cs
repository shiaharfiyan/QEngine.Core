using QEngine.Core.Abstractions;
using System.Collections.Concurrent;

namespace QEngine.Core.Storages
{
    public class MemoryStorage : IStorage
    {
        private ConcurrentDictionary<string, QItem> _items;

        public MemoryStorage()
        {
            _items = new();
        }

        public IEnumerable<QItem> Items
        {
            get
            {
                return _items.Values;
            }
        }

        public string Name => "Memory";

        public string Connection { get; set; } = string.Empty;

        public bool Acknowledge(string id)
        {
            if (_items.ContainsKey(id))
            {
                var item = _items[id];
                item.IsAcknowledged = true;
                return true;
            }

            return false;
        }

        public bool Add(QItem o)
        {
            if (!_items.ContainsKey(o.Id))
                return _items.TryAdd(o.Id, o);

            return false;
        }

        public int Commit()
        {
            var counter = 0;
            var notProcessedItems = _items.Values.Where(x => !x.IsCommitted);
            foreach (var item in notProcessedItems)
            {
                var i = _items[item.Id];
                i.IsCommitted = true;
                i.CommitedBy = "Storage:Commit All";
                _items[item.Id] = i;
                counter++;
            }
            return counter;
        }

        public bool Commit(string id, string commitedBy)
        {
            if (_items.ContainsKey(id))
            {
                var item = _items[id];
                item.IsCommitted = true;
                item.CommitedBy = commitedBy;
                return true;
            }

            return false;
        }

        public QItem Get(string id)
        {
            if (_items.ContainsKey(id))
                return _items[id];

            return new QItem();
        }

        public QItem Remove(string id)
        {
            QItem qItem = new();
            if (_items.ContainsKey(id))
                _items.TryRemove(id, out qItem);

            return qItem;
        }
    }
}