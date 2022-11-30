namespace QEngine.Core.EventArgs
{
    public static class QEventHandlers
    {
        public delegate void ConnectedEventHandler(string id);
        public delegate void DisconnectedEventHandler();
        public delegate void ReconnectingEventHandler(int currentRetry, int retryLimit);
        public delegate void ReconnectedEventHandler(string id, DateTime time);

        public delegate void QueueChangedEventHandler(QChangedEventArgs e);
        public delegate void QueueBehaviourChangedEventHandler(QBehavioursChangedEventArgs e);
        public delegate void ClientChangedEventHandler(QClientEventArgs e);

        public delegate void QueueItemChangedEventHandler(QItemEventArgs e);
        public delegate void BindersChangedEventHandler(QBinderEventArgs e);
        public delegate void SubscribersChangedEventHandler(QSubscriberEventArgs e);
        public delegate void CommittedEventHandler(QItemOriginEventArgs e);
        public delegate void WaitingForCommittedEventHandler(QItemOriginEventArgs e);

        public delegate void ReceivedEventHandler(QItemOriginEventArgs e);
        public delegate void AcknowledgedEventHandler(QItemOriginEventArgs e);
        public delegate void WaitingForAcknowledgedEventHandler(QItemOriginEventArgs e);
        public delegate void QueueThrottlingEventHandler(QThrottlingEventArgs e);
        public delegate void TimeArrivedEventHandler(DateTime dateTime);
    }
}
