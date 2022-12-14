using SignalMQ.Core.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Storages
{
    public abstract class QueueStorage<T> : IQueueStorage<T> where T : IQueueStorageRecord
    {
        public string Name { get; init; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public virtual bool Alter(string queueName, T queue)
        {
            return Task.Run(async () => await AlterAsync(queueName, queue))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> AlterAsync(string queueName, T queue);

        public virtual bool AlterBehaviour(string queueName, Behaviour behaviour)
        {
            return Task.Run(async () => await AlterBehaviourAsync(queueName, behaviour))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> AlterBehaviourAsync(string queueName, Behaviour behaviour);

        public virtual bool AlterCapacity(string queueName, uint capacity)
        {
            return Task.Run(async () => await AlterCapacityAsync(queueName, capacity))
                .GetAwaiter()
                .GetResult();
        }

        public abstract Task<bool> AlterCapacityAsync(string queueName, uint capacity);

        public virtual bool AlterName(string queueName, string name)
        {
            return Task.Run(async () => await AlterNameAsync(queueName, name))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> AlterNameAsync(string queueName, string name);

        public virtual bool AlterProperties(string queueName, Properties properties)
        {
            return Task.Run(async () => await AlterPropertiesAsync(queueName, properties))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> AlterPropertiesAsync(string queueName, Properties properties);

        public virtual IEnumerable<string> GetQueueNames()
        {
            return Task.Run(GetQueueNamesAsync)
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<IEnumerable<string>> GetQueueNamesAsync();

        public virtual IEnumerable<T> GetQueues()
        {
            return Task.Run(GetQueuesAsync)
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<IEnumerable<T>> GetQueuesAsync();

        public bool Remove(string queueName)
        {
            return Task.Run(async () => await RemoveAsync(queueName))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> RemoveAsync(string queueName);

        public bool Store(T queue)
        {
            return Task.Run(async () => await StoreAsync(queue))
                .GetAwaiter()
                .GetResult();
        }
        public abstract Task<bool> StoreAsync(T queue);
    }
}