namespace QEngine.Core.Values
{
    public struct TimeOutValue
    {
        public TimeOutValue(int configuredTimeOut)
        {
            this.ConfiguredTimeOut = configuredTimeOut;
        }

        public object Value => "TimeOut";

        public int ConfiguredTimeOut { get; init; }
    }
}
