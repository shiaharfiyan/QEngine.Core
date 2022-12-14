using SignalMQ.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Abstractions
{
    public interface IClientManager<T> where T : IClientInfo, new()
    {
        event ClientChangedEventHandler? QueueClientChanged;

        T[] Clients { get; }

        void Register(T clientInfo);
        Task RegisterAsync(T clientInfo);

        void Unregister(T clientInfo);
        Task UnregisterAsync(T clientInfo);

        bool Exists(string connectionId);
        Task<bool> ExistsAsync(string connectionId);
        
        bool UpdateHealthCheck(string connectionId, DateTime dateTime);
        Task<bool> UpdateHealthCheckAsync(string connectionId, DateTime dateTime);

        bool TryGet(string connectionId, out T clientInfo);

        void Set(string connectionId, T clientInfo);
        Task SetAsync(string connectionId, T clientInfo);

        AbortResult Abort(string connectionId);
        Task<AbortResult> AbortAsync(string connectionId);
    }
}
