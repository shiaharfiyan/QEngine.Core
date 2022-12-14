using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace SignalMQ.Core.Extensions
{
    public static class HubConnectionExtension
    {
        public static bool StartAsync(this HubConnection _hubConnection, int retryMs, CancellationToken token)
        {
            bool result = false;

            Task.Run(async () =>
            {
                result = await _hubConnection.ConnectRetryAsync(token, retryMs);
            }, token).Wait(token);

            return result;
        }

        private static async Task<bool> ConnectRetryAsync(this HubConnection _hubConnection, CancellationToken token, int retryMs = 5000)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await _hubConnection.StartAsync(token);
                    Debug.Assert(_hubConnection.State == HubConnectionState.Connected);
                    return true;
                }
                catch when (token.IsCancellationRequested)
                {
                    return false;
                }
                catch (Exception e)
                {
                    if (_hubConnection.State != HubConnectionState.Connected)
                    {
                        // Failed to connect, trying again in 5000 ms
                        Console.WriteLine($"Error: {e.Message}");
                        Console.WriteLine($"Connection failed. Retrying after {retryMs / 1000f:0.000}s...");
                        await Task.Delay(retryMs);

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Back Online");
                        return true;
                    }
                }
            }
        }
    }
}