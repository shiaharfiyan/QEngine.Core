namespace SignalMQ.Core.Values
{
    public struct ThrottledValue
    {
        public ThrottledValue(Throttling throttling)
        {
            Throttling = throttling;
            Name = $"Throttled.{throttling}";
        }

        public string Name { get; set; } = "Throttled";

        public Throttling Throttling { get; set; }

        public string Type => typeof(ThrottledValue).Name;
    }
}