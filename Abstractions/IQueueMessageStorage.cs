using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueMessageStorage<T> where T : IQueueMessageStorageRecord
    {
        string Name { get; init; }
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }

        long RecordCount(string queueName);
        Task<long> RecordCountAsync(string queueName);

        void DropCollection(string queueName);
        Task DropCollectionAsync(string queueName);

        void Store(string queueName, T o);
        Task StoreAsync(string queueName, T o);

        bool Remove(string queueName, string id);
        Task<bool> RemoveAsync(string queueName, string id);

        T Get(string queueName, string id);
        Task<T> GetAsync(string queueName, string id);

        Dictionary<string, IEnumerable<T>> GetNotCommitted();
        Task<Dictionary<string, IEnumerable<T>>> GetNotCommittedAsync();

        IEnumerable<T> GetNotCommitted(string queueName);
        Task<IEnumerable<T>> GetNotCommittedAsync(string queueName);

        void Acknowledge(string queueName, string id);
        Task AcknowledgeAsync(string queueName, string id);

        void Commit(string queueName, string id, string commitedBy);
        Task CommitAsync(string queueName, string id, string commitedBy);

        long Commit(string queueName);
        Task<long> CommitAsync(string queueName);
    }
}