using SignalMQ.Core.Abstractions;

namespace SignalMQ.Core.EventArgs
{
    public class ClientEventArgs
    {
        public ClientEventArgs(string id, IClientInfo client, int oldCount, int newCount)
        {
            Id = id;
            Client = client;
            OldCount = oldCount;
            NewCount = newCount;
        }

        public string Id { get; init; }

        public IClientInfo Client { get; init; }

        public int OldCount { get; init; }
        public int NewCount { get; internal set; }
    }
}