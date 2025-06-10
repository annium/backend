using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Mesh.Client;

/// <summary>
/// Extension methods for mesh client interfaces providing async connection utilities
/// </summary>
public static class ClientExtensions
{
    /// <summary>
    /// Connects the client and returns a task that completes when the connection is established
    /// </summary>
    /// <param name="client">The client to connect</param>
    /// <returns>A task that completes when the client is connected</returns>
    public static Task ConnectAsync(this IClient client)
    {
        var tcs = new TaskCompletionSource();

        client.Trace("subscribe to OnConnected");
        client.OnConnected += HandleConnected;

        client.Trace("connect");
        client.Connect();

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
