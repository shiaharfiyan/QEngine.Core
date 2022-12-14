namespace SignalMQ.Core.Utilities
{
    public static class QTask
    {
        public static void FireAndForget(Action action)
        {
            Task.Run(() =>
            {
                action.Invoke();
            });
        }
        public static void FireAndForget(Task<Action> action)
        {
            Task.Run(async () =>
            {
                await action;
            });
        }
    }
}