using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SignalMQ.Core.EventArgs.QueueEventHandlers;

namespace SignalMQ.Core.Abstractions
{
    public interface IQueueManager<T> where T : IQueue
    {
        event QueueChangedEventHandler? QueueChanged;

        event QueueItemChangedEventHandler? QueueItemChanged;
        event AttachersChangedEventHandler? QueueAttacherChanged;

        event QueueThrottlingEventHandler? QueueItemRejected;
        event QueueThrottlingEventHandler? QueueItemIgnored;

        event QueueBehaviourChangedEventHandler? QueueBehaviourChanged;

        T[] Queues { get; }

        bool Create(string queueName);
        Task<bool> CreateAsync(string queueName);

        bool Create(T queue);
        Task<bool> CreateAsync(T queue);

        bool Alter(string queueName, T queue);
        Task<bool> AlterAsync(string queueName, T queue);

        bool AlterName(string queueName, string name);
        Task<bool> AlterNameAsync(string queueName, string name);

        bool AlterBehaviour(string queueName, Behaviour behaviour);
        Task<bool> AlterBehaviourAsync(string queueName, Behaviour behaviour);

        bool AlterProperties(string queueName, Properties properties);
        Task<bool> AlterPropertiesAsync(string queueName, Properties properties);

        bool Exists(string queueName);
        Task<bool> ExistsAsync(string queueName);

        bool Exists(T queue);
        Task<bool> ExistsAsync(T queue);

        bool TryGet(string queueName, out T queue);

        bool Remove(string queueName);
        Task<bool> RemoveAsync(string queueName);
    }
}