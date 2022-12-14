namespace SignalMQ.Core.Values
{
    public struct OkValue
    {
        public string Name => "Ok";

        public string Type => typeof(OkValue).Name;
    }
}