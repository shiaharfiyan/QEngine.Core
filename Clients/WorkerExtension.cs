using SignalMQ.Core.Abstractions;

namespace SignalMQ.Core.Clients
{
    public static class WorkerExtension
    {
        public static async Task<bool> DoSendAsync(this IWorker worker, Func<Task<bool>> task, CancellationTokenSource? token = null)
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

            var result = await Task.Run(task.Invoke).WaitAsync(token?.Token ?? new CancellationTokenSource().Token);
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
