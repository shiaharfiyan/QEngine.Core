namespace SignalMQ.Core.Exceptions
{
    public class NotConnectedException : Exception
    {
        private string _host;

        public NotConnectedException(string host)
        {
            _host = host;
        }

        public override string Message => $"Not connected to server '{_host}', ensure connection has been established.";
    }
}
