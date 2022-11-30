using Lifecare.Helper;

namespace QEngine.Core
{
    public class QChannel
    {
        public QChannel(string name)
        {
            Id = Randomizer.Generate(15);
            Name = name;
        }

        public QChannel(string name, string queueName) : this(name)
        {
            QueueName = queueName;
        }

        public string Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;

        public bool Persistent { get; set; }
        public bool AutoDelete { get; set; }

        public QAcknowledgement Acknowledge { get; set; } = QAcknowledgement.Automatic;
        public QAcknowledgement Commit { get; set; } = QAcknowledgement.Automatic;

        public QItem Create(object o)
        {
            QItemOptions options = new()
            {
                Persistent = Persistent,
                AutoDelete = AutoDelete,

                Acknowledge = Acknowledge,
                Commit = Commit
            };

            return new QItem(o) { Options = options };
        }
    }
}