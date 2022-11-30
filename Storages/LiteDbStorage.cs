using LiteDB;
using QEngine.Core.Abstractions;

namespace QEngine.Core.Storages
{
    public class LiteDbStorage : IStorage
    {
        public IEnumerable<QItem> Items
        {
            get
            {
                using var db = new LiteDatabase(Connection);
                var qItems = db.GetCollection<QItem>("queueItems");
                return qItems.FindAll();
            }
        }

        public string Name => "LiteDb";

        public string Connection { get; set; } = "DbLite.db";

        public bool Acknowledge(string id)
        {
            try
            {
                using var db = new LiteDatabase(Connection);
                var qItems = db.GetCollection<QItem>("queueItems");
                var oExisting = qItems.FindOne($"$.Id = '{id}'");
                oExisting.IsAcknowledged = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Commit(string id, string commitedBy)
        {
            try
            {
                using var db = new LiteDatabase(Connection);
                var qItems = db.GetCollection<QItem>("queueItems");
                var oExisting = qItems.FindOne($"$.Id = '{id}'");
                oExisting.IsCommitted = true;
                oExisting.CommitedBy = commitedBy;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Add(QItem o)
        {
            try
            {
                using var db = new LiteDatabase(Connection);
                var qItems = db.GetCollection<QItem>("queueItems");
                qItems.Insert(o);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public QItem Remove(string id)
        {
            using var db = new LiteDatabase(Connection);
            var qItems = db.GetCollection<QItem>("queueItems");
            var oExisting = qItems.FindOne($"$.Id = '{id}'");
            var affectedCount = qItems.DeleteMany($"$.Id = '{id}'");
            oExisting.IsRemoved = true;
            return oExisting;
        }

        public QItem Get(string id)
        {
            using var db = new LiteDatabase(Connection);
            var qItems = db.GetCollection<QItem>("queueItems");
            return qItems.FindOne($"$.Id = '{id}'");
        }

        public int Commit()
        {
            using var db = new LiteDatabase(Connection);
            var qItems = db.GetCollection<QItem>("queueItems");
            var oExistings = qItems.Find($"$.IsCommitted = false").ToArray();
            var count = 0;
            for (int i = 0; i < oExistings.Length; i++)
            {
                var o = oExistings[i];
                o.IsCommitted = true;
                o.CommitedBy = "Storage:Commit All";
                qItems.Update(oExistings[i]);
                count++;
            }

            return count;
        }
    }
}
