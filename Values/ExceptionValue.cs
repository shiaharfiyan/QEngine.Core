namespace QEngine.Core.Values
{
    public struct ExceptionValue
    {
        public ExceptionValue(string exceptionMsg)
        {
            Exception = exceptionMsg;
        }

        public object Value => "Exception";
        public string Exception { get; set; } = string.Empty;
    }
}
