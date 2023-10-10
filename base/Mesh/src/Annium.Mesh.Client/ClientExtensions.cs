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
}