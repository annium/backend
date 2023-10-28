using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Mesh.Client;

public static class ClientExtensions
{
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
