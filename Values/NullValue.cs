namespace SignalMQ.Core.Values
{
    public struct NullValue
    {
        public string Name => "Null";

        public string Type => typeof(NullValue).Name;
    }
}