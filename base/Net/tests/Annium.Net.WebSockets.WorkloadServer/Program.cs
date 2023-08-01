using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Net.WebSockets;

const string workloadMessage = "{{\"stream\":\"{0}@aggTrade\",\"data\":{{\"e\":\"aggTrade\",\"E\":1689659049498,\"s\":\"{1}\",\"a\":2675370021,\"p\":\"30048.53000000\",\"q\":\"0.00332000\",\"f\":3174123265,\"l\":3174123265,\"T\":1689659049497,\"m\":false,\"M\":true}}}}";

await using var entry = Entrypoint.Default.Setup();

var port = args.Length > 0 ? int.Parse(args[0]) : 9898;
var totalMessages = args.Length > 1 ? int.Parse(args[1]) : 100_000;

var server = new WebSocketServer(new IPEndPoint(IPAddress.Loopback, port), "/", HandleClient);
await server.RunAsync(entry.Ct);

async Task HandleClient(WebSocket rawSocket, CancellationToken ct)
{
    var clientSocket = new ManagedWebSocket(rawSocket);

    ReadOnlyMemory<byte> workloadMessageBytes = Encoding.UTF8.GetBytes(workloadMessage).AsMemory();
    for (var i = 0; i < totalMessages; i++)
        await clientSocket.SendTextAsync(workloadMessageBytes).ConfigureAwait(false);

    await rawSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}