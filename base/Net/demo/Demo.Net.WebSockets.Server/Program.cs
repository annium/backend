using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Net.Servers;

await using var entry = Entrypoint.Default.Setup();

var server = WebServerBuilder.New(new Uri("http://127.0.0.1:9898")).WithWebSockets(HandleWebSocket).Build();
await server.RunAsync(entry.Ct);
return;

static async Task HandleWebSocket(HttpListenerWebSocketContext ctx, CancellationToken ct)
{
    await ctx.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}