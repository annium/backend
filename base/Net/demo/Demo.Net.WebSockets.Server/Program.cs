using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Net.Servers;

await using var entry = Entrypoint.Default.Setup();

var server = new WebServer(new IPEndPoint(IPAddress.Loopback, 9898), "/", HandleClient);
await server.RunAsync(entry.Ct);

static async Task HandleClient(WebSocket rawSocket, CancellationToken ct)
{
    await rawSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}