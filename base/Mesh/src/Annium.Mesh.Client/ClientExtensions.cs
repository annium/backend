using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Mesh.Client;

/// <summary>
/// Extension methods for mesh client interfaces providing async connection utilities
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Connects the client and returns a task that completes when the connection is established.
    /// The wait is bounded by the client's <see cref="IClientBase.ConnectTimeout"/>: the underlying
    /// transport retries connection attempts indefinitely, so without this bound a client that can
    /// never reach the server (or is starved) would hang forever. On timeout the client is
    /// disconnected (stopping the retry loop) and a <see cref="TimeoutException"/> is thrown.
    /// </summary>
    /// <param name="client">The client to connect</param>
    /// <returns>A task that completes when the client is connected</returns>
    public static async Task ConnectAsync(this IClient client)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        client.Trace("subscribe to OnConnected");
        client.OnConnected += HandleConnected;

        client.Trace("connect");
        client.Connect();

        try
        {
            client.Trace("await connection (bounded by connect timeout)");
            await tcs.Task.WaitAsync(client.ConnectTimeout.ToTimeSpan());
        }
        catch (TimeoutException)
        {
            client.Trace("connect timed out - unsubscribe and disconnect");
            client.OnConnected -= HandleConnected;
            client.Disconnect();
            throw;
        }

        void HandleConnected()
        {
            client.Trace("unsubscribe from OnConnected");
            client.OnConnected -= HandleConnected;

            client.Trace("try set result");
            tcs.TrySetResult();

            client.Trace("done");
        }
    }

    /// <summary>
    /// Returns a task that completes when the managed client is connected
    /// </summary>
    /// <param name="client">The managed client to wait for connection</param>
    /// <returns>A task that completes when the client is connected</returns>
    public static Task WhenConnectedAsync(this IManagedClient client)
    {
        var tcs = new TaskCompletionSource();

        client.Trace("subscribe to OnConnected");
        client.OnConnected += HandleConnected;

        client.Trace("return task");

        return tcs.Task;

        void HandleConnected()
        {
            client.Trace("unsubscribe from OnConnected");
            client.OnConnected -= HandleConnected;

            client.Trace("try set result");
            tcs.TrySetResult();

            client.Trace("done");
        }
    }
}
