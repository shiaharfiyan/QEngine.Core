namespace SignalMQ.Core
{
    /// <summary>
    /// Acknowledgement used for message properties Acknowledge and Commit
    /// </summary>
    public enum Acknowledgement
    {
        None,
        AutoServerSide,
        AutoClientSide,
        Manual
    }
}