using SignalMQ.Core.Values;

namespace SignalMQ.Core.Results
{
    public struct CallBackResult
    {
        public CallBackResult(bool isSucceeded, object value)
        {
            IsSucceeded = isSucceeded;
            Value = value;
        }

        public object Value { get; set; }

        public bool IsSucceeded { get; set; }

        public bool IsOk
        {
            get { return Value != null && Value is OkValue; }
        }

        public OkValue AsOk
        {
            get { return (OkValue)Value; }
        }

        public bool IsNull
        {
            get { return Value != null && Value is NullValue; }
        }

        public NullValue AsNull
        {
            get { return (NullValue)Value; }
        }

        public bool IsException
        {
            get { return Value != null && Value is ExceptionValue; }
        }

        public ExceptionValue AsException
        {
            get { return (ExceptionValue)Value; }
        }

        public bool IsTimeOut
        {
            get { return Value != null && Value is TimeOutValue; }
        }

        public TimeOutValue AsTimeOut
        {
            get { return (TimeOutValue)Value; }
        }

        public bool IsThrottled
        {
            get { return Value != null && Value is ThrottledValue; }
        }

        public ThrottledValue AsThrottled
        {
            get { return (ThrottledValue)Value; }
        }

        public bool IsAborted
        {
            get { return Value != null && Value is AbortValue; }
        }

        public AbortValue AsAbort
        {
            get { return (AbortValue)Value; }
        }
    }
}