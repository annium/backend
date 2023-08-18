using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers;

namespace Annium.Net.WebSockets.Tests;

public abstract class TestBase
{
    protected readonly Uri ServerUri = new($"ws://127.0.0.1:{new Random().Next(32768, 65536)}");

    protected IAsyncDisposable RunServer(Func<WebSocket, CancellationToken, Task> handleClient)
    {
        var server = new WebServer(new IPEndPoint(IPAddress.Loopback, ServerUri.Port), "/", handleClient);
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            cts.Cancel();
            await serverTask;
        });
    }
}