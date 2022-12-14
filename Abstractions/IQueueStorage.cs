using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueStorage<T> where T : IQueueStorageRecord
    {
        string Name { get; init; }
        string DatabaseName { get; set; }
        string CollectionName { get; set; }
        string ConnectionString { get; set; }

        IEnumerable<string> GetQueueNames();
        Task<IEnumerable<string>> GetQueueNamesAsync();

        IEnumerable<T> GetQueues();
        Task<IEnumerable<T>> GetQueuesAsync();

        bool Store(T queue);
        Task<bool> StoreAsync(T queue);

        bool Remove(string queueName);
        Task<bool> RemoveAsync(string queueName);

        bool Alter(string queueName, T queue);
        Task<bool> AlterAsync(string queueName, T queue);

        bool AlterName(string queueName, string name);
        Task<bool> AlterNameAsync(string queueName, string name);

        bool AlterCapacity(string queueName, uint capacity);
        Task<bool> AlterCapacityAsync(string queueName, uint capacity);

        bool AlterBehaviour(string queueName, Behaviour behaviour);
        Task<bool> AlterBehaviourAsync(string queueName, Behaviour behaviour);

        bool AlterProperties(string queueName, Properties properties);
        Task<bool> AlterPropertiesAsync(string queueName, Properties properties);
    }
}