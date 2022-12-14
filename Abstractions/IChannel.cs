using Microsoft.AspNetCore.SignalR.Client;

namespace SignalMQ.Core.Abstractions
{
    public interface IChannel
    {
        string QueueName { get; }
        Uri BaseUri { get; }

        HubConnection BuildHub();
        Task<string?> GetAccessTokenProviderAsync();

        bool Persistent { get; set; }

        Acknowledgement Acknowledge { get; set; }
        Acknowledgement Commit { get; set; }

        Message<T> Create<T>(object o);
        Task<Message<T>> CreateAsync<T>(object o);
    }
}