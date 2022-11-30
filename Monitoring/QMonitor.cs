using LiteDB;

namespace QEngine.Core.Monitoring
{
    public class QMonitor
    {
        public QMonitor()
        {

        }

        public void StartCommittedItem(string queueName, string id)
        {
            using var db = new LiteDatabase("qmon.db");
            var qMonItems = db.GetCollection<QMonitorItem>($"{queueName}_{nameof(QMonitorItem)}");
            var qMonItem = new QMonitorItem(id, queueName);

            qMonItems.Insert(qMonItem);
        }

        public void EndCommittedItem(string queueName, string id)
        {
            using var db = new LiteDatabase("qmon.db");
            var qMonItems = db.GetCollection<QMonitorItem>($"{queueName}_{nameof(QMonitorItem)}");
            var qMonItem = qMonItems.FindOne(x => x.Id == id);

            qMonItem.End = DateTime.Now;
            qMonItem.IsFinished = true;

            qMonItems.Update(qMonItem);
        }

        public double GetCommittedDuration(string queueName, string id)
        {
            using var db = new LiteDatabase("qmon.db");
            var qMonItems = db.GetCollection<QMonitorItem>($"{queueName}_{nameof(QMonitorItem)}");
            var qMonItem = qMonItems.FindOne(x => x.Id == id);

            return qMonItem.Duration;
        }

        public QMonitorItem[] GetItems(string[] queueNames)
        {
            var qMonTotalItems = new List<QMonitorItem>();
            using var db = new LiteDatabase("qmon.db");
            foreach (var queueName in queueNames)
            {
                var qMonItems = db.GetCollection<QMonitorItem>($"{queueName}_{nameof(QMonitorItem)}");
                qMonTotalItems.AddRange(qMonItems.FindAll());
            }

            return qMonTotalItems.ToArray();
        }
    }
}
