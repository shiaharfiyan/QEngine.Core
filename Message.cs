using Newtonsoft.Json;
using SignalMQ.Core.Abstractions;
using SignalMQ.Core.Utilities;
using SignalMQ.Core.Values;

namespace SignalMQ.Core
{
    public struct Message
    {
        //1
        public Message(string id, string json, string type, Properties properties)
        {
            Id = id;
            ValueAsJson = json;
            ValueType = type;
            Properties = properties;
        }

        //2
        //Call 1
        public Message(string json, string type, Properties properties)
            : this(Randomizer.Generate(32), json, type, properties)
        {

        }

        //3
        //Call 1
        public Message(string json)
            : this(Randomizer.Generate(32), json, new Properties())
        {

        }

        //4
        //Call 2
        public Message(object data, Properties properties)
            : this(JsonConvert.SerializeObject(data), data == null ? "null" : data.GetType().Name, properties)
        {

        }

        //5
        //Call 3
        public Message(object data)
            : this(JsonConvert.SerializeObject(data))
        {
            if (data != null)
                ValueType = data.GetType().Name;
            else
                ValueType = "null";
        }

        public string Id { get; internal set; }

        public string ValueAsJson { get; internal set; }

        public string ValueType { get; internal set; }

        public Properties Properties { get; internal set; }

        public Message<T> ToMessage<T>()
        {
            return new Message<T>(Id, this.ValueAs<T>(), Properties);
        }

        public static Message Exception(Exception e)
        {
            return new Message(new ExceptionValue(e));
        }

        public static Message Null()
        {
            return new Message(new NullValue());
        }

        public static Message Ok()
        {
            return new Message(new OkValue());
        }

        public static Message Throttled(Throttling throttling)
        {
            return new Message(new ThrottledValue(throttling));
        }

        public static Message TimeOut(int timeOutMs)
        {
            return new Message(new TimeOutValue(timeOutMs));
        }

        public static Message Abort()
        {
            return new Message(new AbortValue());
        }

        public Message Clone()
        {
            return new Message(Id, ValueAsJson, Properties);
        }
    } 

    public struct Message<T>
    {
        public Message(string id, T? data, Properties properties)
        {
            Id = id;
            Value = data;
            Properties = properties;
        }

        public string Id { get; }

        public T? Value { get; }

        public Properties Properties { get; }

        public Message ToMessage()
        {
            return new Message(Id, JsonConvert.SerializeObject(Value), Value == null ? "null" : Value.GetType().Name, Properties);
        }
    }
}