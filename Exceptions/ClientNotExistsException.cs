namespace SignalMQ.Core.Exceptions
{
    public class ClientNotExistsException : Exception
    {
        private string _connectionId = string.Empty;

        public ClientNotExistsException(string connectionId)
        {
            _connectionId = connectionId;
        }

        public override string Message => $"Client with Id '{_connectionId}' is not exist";
    }
}
