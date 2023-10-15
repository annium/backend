using System.Threading.Tasks;

namespace Annium.Mesh.Client;

public static class ClientExtensions
{
    public static Task ConnectAsync(this IClient client)
    {
        var tcs = new TaskCompletionSource();

        client.OnConnected += HandleConnected;
        client.Connect();

        return tcs.Task;

        void HandleConnected()
        {
            tcs.SetResult();
            client.OnConnected -= HandleConnected;
        }
    }

    public static Task WhenConnectedAsync(this IManagedClient client)
    {
        var tcs = new TaskCompletionSource();

        client.OnConnected += HandleConnected;

        return tcs.Task;

        void HandleConnected()
        {
            tcs.SetResult();
            client.OnConnected -= HandleConnected;
        }
    }
}