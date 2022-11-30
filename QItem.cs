using Lifecare.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QEngine.Core.Values;

namespace QEngine.Core
{
    public struct QItem
    {
        public QItem(object? data = null)
        {
            Id = Randomizer.Generate(32);

            if (data == null)
                Value = new NullValue();
            else
                Value = data;

            Options = new();
            ValueType = Value.GetType();
        }

        public string Id { get; set; }
        public object Value { get; set; }

        public object? GetValue()
        {
            if (Value == null)
            {
                Value = new JObject(new NullValue());
                ValueType = typeof(NullValue);
            }

            if (ValueType.IsPrimitive || Value is decimal || Value is double || Value is float || Value is string)
                return Value;

            return ((JObject)Value).ToObject(ValueType);
        }

        public Type ValueType { get; set; }

        public bool IsCommitted { get; set; } = false;
        public bool IsAcknowledged { get; set; } = false;
        public bool IsRemoved { get; set; } = false;

        public string CommitedBy { get; set; } = string.Empty;

        public QItemOptions Options { get; set; }

        [JsonIgnore]
        public bool IsTimeOut => ValueType.Equals(typeof(TimeOutValue));
        [JsonIgnore]
        public bool IsNullOrEmpty => Value == null || ValueType.Equals(typeof(NullValue)) || ValueType.Equals(typeof(int)) && Value is int value && value == 0;
        [JsonIgnore]
        public bool IsException => ValueType.Equals(typeof(ExceptionValue));
        [JsonIgnore]
        public bool IsRequestCancel => ValueType.Equals(typeof(RequestCancelValue));

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}