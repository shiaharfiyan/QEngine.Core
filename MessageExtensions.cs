using Newtonsoft.Json;
using SignalMQ.Core.Abstractions;
using SignalMQ.Core.Storages;
using SignalMQ.Core.Values;

namespace SignalMQ.Core
{
    public static class MessageExtensions
    {
        public static T? ValueAs<T>(this Message message)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(message.ValueAsJson);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static object? ValueAsObject(this Message message)
        {
            try
            {
                return JsonConvert.DeserializeObject(message.ValueAsJson);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Message<T> ToMessage<T>(this Message message)
        {
            return new Message<T>(message.Id, message.ValueAs<T>(), message.Properties);
        }

        public static bool IsTimeOut(this Message message)
        {
            try
            {
                return message.ValueType.Equals(typeof(TimeOutValue).Name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsException(this Message message)
        {
            try
            {
                return message.ValueType.Equals(typeof(ExceptionValue).Name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsAbort(this Message message)
        {
            try
            {
                return message.ValueType.Equals(typeof(AbortValue).Name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsOk(this Message message)
        {
            try
            {
                return message.ValueType.Equals(typeof(OkValue).Name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsThrottled(this Message message)
        {
            try
            {
                return message.ValueType.Equals(typeof(ThrottledValue).Name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsNull(this Message message)
        {
            try
            {
                return message.ValueType.Equals(typeof(NullValue).Name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static MessageRecord ToRecord(this Message message)
        {
            return new MessageRecord(message.ValueAsJson, message.ValueType) { Id = message.Id, Properties = message.Properties };
        }
    }
}