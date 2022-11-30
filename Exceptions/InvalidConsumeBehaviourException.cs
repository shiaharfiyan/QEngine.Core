namespace QEngine.Core.Exceptions
{
    public class InvalidConsumeBehaviourException : Exception
    {
        public InvalidConsumeBehaviourException(QThrottling throttling)
        {
            Throttling = throttling;
        }

        public QThrottling Throttling { get; init; }

        public override string Message => $"Invalid throttling '{Throttling}' for consume behaviour. use Block & BlockWithTimeOut instead";
    }
}
