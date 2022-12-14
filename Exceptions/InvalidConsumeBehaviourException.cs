namespace SignalMQ.Core.Exceptions
{
    public class InvalidConsumeBehaviourException : Exception
    {
        public InvalidConsumeBehaviourException(Throttling throttling)
        {
            Throttling = throttling;
        }

        public Throttling Throttling { get; init; }

        public override string Message => $"Invalid throttling '{Throttling}' for consume behaviour. use Block & BlockWithTimeOut instead";
    }
}
