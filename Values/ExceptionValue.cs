namespace SignalMQ.Core.Values
{
    public struct ExceptionValue
    {
        public ExceptionValue(Exception exception)
        {
            Exception = exception.Message;
            Name = $"Exception.{exception.GetType().Name}";
        }

        public string Name { get; set; } = "Exception";
        public string Exception { get; set; } = string.Empty;

        public string Type => typeof(ExceptionValue).Name;
    }
}