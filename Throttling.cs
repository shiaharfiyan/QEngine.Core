namespace SignalMQ.Core
{
    /// <summary>
    /// Queue behaviour for Produce or Consume
    /// </summary>
    public enum Throttling
    {
        Block = 0,
        BlockWithTimeOut = 1,
        Ignore = 2,
        Reject = 3,
    }
}