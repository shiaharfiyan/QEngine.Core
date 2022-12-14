using SignalMQ.Core.Abstractions;

namespace SignalMQ.Core.Clients
{
    public static class QWorkerExtension
    {
        public static async Task<bool> DoSendAsync(this IWorker worker, Func<Task<bool>> task)
        {
            int counter = 0;
            while (!worker.IsConnected)
            {
                if (counter < worker.ConnectionTimeOut)
                {
                    await Task.Delay(1000);
                    counter += 1000;
                }
                else
                {
                    return false;
                }
            }

            var result = await Task.Run(() =>
            {
                return task.Invoke();
            }).WaitAsync(new CancellationToken());
            return result;
        }

        public static bool DoSend(this IWorker worker, Func<bool> task)
        {
            int counter = 0;
            while (!worker.IsConnected)
            {
                if (counter < worker.ConnectionTimeOut)
                {
                    Task.Delay(1000);
                    counter += 1000;
                }
                else
                {
                    return false;
                }
            }

            return task.Invoke();
        }
    }
}
