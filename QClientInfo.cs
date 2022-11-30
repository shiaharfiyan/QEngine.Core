using System.Net;

namespace QEngine.Core
{
    public record QClientInfo
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Origin { get; init; } = Dns.GetHostName();

        public string QueueName { get; set; } = string.Empty;

        public QOrigin Type { get; set; } = QOrigin.Subscriber;

        public DateTime LastEchoTime { get; set; } = DateTime.Now;
    }
}
