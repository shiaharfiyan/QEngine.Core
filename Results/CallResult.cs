using Newtonsoft.Json;
using SignalMQ.Core.Abstractions;
using SignalMQ.Core.Values;

namespace SignalMQ.Core.Results
{
    public struct CallResult : IResult
    {
        public CallResult(bool success, Message value)
        {
            Success = success;
            Message = value;
        }

        public bool Success { get; set; } = false;

        public Message Message { get; set; }
    }

    public static class CallResultExtensions
    {
        public static object Value(this CallResult cr)
        {
            return cr.Message.ValueAsObject() ?? new NullValue();
        }

        public static bool IsTimeOut(this CallResult cr)
        {
            try
            {
                return cr.Message.IsTimeOut();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static TimeOutValue AsTimeOut(this CallResult cr)
        {
            try
            {
                if (IsTimeOut(cr)) return JsonConvert.DeserializeObject<TimeOutValue>(cr.Message.ValueAsJson);

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static bool IsThrottled(this CallResult cr)
        {
            try
            {
                return cr.Message.IsThrottled();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static ThrottledValue AsThrottled(this CallResult cr)
        {
            try
            {
                if (IsThrottled(cr)) return JsonConvert.DeserializeObject<ThrottledValue>(cr.Message.ValueAsJson);

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static bool IsException(this CallResult cr)
        {
            try
            {
                return cr.Message.IsException();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static ExceptionValue AsException(this CallResult cr)
        {
            try
            {
                if (IsException(cr)) return JsonConvert.DeserializeObject<ExceptionValue>(cr.Message.ValueAsJson);

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static bool IsAbort(this CallResult cr)
        {
            try
            {
                return cr.Message.IsAbort();
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static AbortValue AsAbort(this CallResult cr)
        {
            try
            {
                if (IsAbort(cr)) return JsonConvert.DeserializeObject<AbortValue>(cr.Message.ValueAsJson);

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}