using SignalMQ.Core.Results;

namespace SignalMQ.Core.EventArgs
{
    public static class QueueEventHandlers
    {
        public delegate void QueueChangedEventHandler(QueueChangedEventArgs e);
        public delegate void QueueBehaviourChangedEventHandler(QueueBehavioursChangedEventArgs e);
        public delegate void ClientChangedEventHandler(ClientEventArgs e);

        public delegate void QueueItemChangedEventHandler(QueueMessageEventArgs e);
        public delegate void AttachersChangedEventHandler(AttacherEventArgs e);

        public delegate void ConnectedEventHandler(string id);
        public delegate void DisconnectedEventHandler();
        public delegate void ReconnectingEventHandler(int currentRetry, int retryLimit);
        public delegate void ReconnectedEventHandler(string id, DateTime time);

        public delegate void CommittedEventHandler(CommittedEventArgs e);
        public delegate void WaitingForCommittedEventHandler(MessageEventArgs e);

        public delegate void ReceivedEventHandler(MessageEventArgs e);

        public delegate void AcknowledgedEventHandler(AcknowledgedEventArgs e);
        public delegate void WaitingForAcknowledgedEventHandler(MessageEventArgs e);

        public delegate void QueueThrottlingEventHandler(QueueThrottlingEventArgs e);
        public delegate void TimeArrivedEventHandler(DateTime dateTime);

        public delegate void CallBackDelegate(CallBackResult result);
    }
}
