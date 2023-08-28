using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Logging;
using Annium.Net.Servers;

await using var entry = Entrypoint.Default.Setup();

var server = WebServerBuilder.New(new Uri("http://127.0.0.1:9898")).WithWebSockets(HandleWebSocket).Build(entry.Provider.Resolve<ILogger>());
await server.RunAsync(entry.Ct);
return;

static async Task HandleWebSocket(HttpListenerWebSocketContext ctx, ILogger logger, CancellationToken ct)
{
    await ctx.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}