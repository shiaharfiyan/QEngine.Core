namespace SignalMQ.Core.Values
{
    public struct AbortValue
    {
        public string Name => "Abort";

        public string Type => typeof(AbortValue).Name;
    }
}