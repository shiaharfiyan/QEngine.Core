using SignalMQ.Core;
using SignalMQ.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Storages
{
    public abstract class MessageStorage<T> : IQueueMessageStorage<T> where T : IQueueMessageStorageRecord
    {
        public string Name { get; init; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public virtual void Acknowledge(string queueName, string id)
        {
            Task.Run(async () => await AcknowledgeAsync(queueName, id)).Wait();
        }
        public abstract Task AcknowledgeAsync(string queueName, string id);

        public virtual void Commit(string queueName, string id, string commitedBy)
        {
            Task.Run(async () => await CommitAsync(queueName, id, commitedBy)).Wait();
        }
        public abstract Task CommitAsync(string queueName, string id, string commitedBy);

        public virtual long Commit(string queueName)
        {
            return Task.Run(async () => await CommitAsync(queueName))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<long> CommitAsync(string queueName);

        public virtual void DropCollection(string queueName)
        {
            Task.Run(async () => await DropCollectionAsync(queueName)).Wait();
        }
        public abstract Task DropCollectionAsync(string queueName);

        public virtual T Get(string queueName, string id)
        {
            return Task.Run(async () => await GetAsync(queueName, id))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<T> GetAsync(string queueName, string id);

        public virtual Dictionary<string, IEnumerable<T>> GetNotCommitted()
        {
            return Task.Run(GetNotCommittedAsync)
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<Dictionary<string, IEnumerable<T>>> GetNotCommittedAsync();

        public virtual IEnumerable<T> GetNotCommitted(string queueName)
        {
            return Task.Run(async () => await GetNotCommittedAsync(queueName))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<IEnumerable<T>> GetNotCommittedAsync(string queueName);

        public virtual long RecordCount(string queueName)
        {
            return Task.Run(async () => await RecordCountAsync(queueName))
                .GetAwaiter()
                .GetResult();
        }

        public abstract Task<long> RecordCountAsync(string queueName);

        public virtual bool Remove(string queueName, string id)
        {
            return Task.Run(async () => await RemoveAsync(queueName, id))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> RemoveAsync(string queueName, string id);

        public virtual void Store(string queueName, T o)
        {
            Task.Run(async () => await StoreAsync(queueName, o));
        }
        public abstract Task StoreAsync(string queueName, T o);
    }
}
