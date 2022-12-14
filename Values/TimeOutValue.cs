namespace SignalMQ.Core.Values
{
    public struct TimeOutValue
    {
        public TimeOutValue(int configuredTimeOut)
        {
            ConfiguredTimeOut = configuredTimeOut;
            Name = $"TimeOut.{configuredTimeOut}";
        }

        public string Name { get; set; } = "TimeOut";

        public int ConfiguredTimeOut { get; set; } = -1;

        public string Type => typeof(TimeOutValue).Name;
    }
}