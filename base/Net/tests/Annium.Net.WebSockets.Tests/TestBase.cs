using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers;

namespace Annium.Net.WebSockets.Tests;

public abstract class TestBase
{
    private readonly int _port;
    protected readonly Uri ServerUri;

    protected TestBase()
    {
        _port = new Random().Next(32768, 65536);
        ServerUri = new Uri($"ws://127.0.0.1:{_port}");
    }

    protected IAsyncDisposable RunServer(Func<HttpListenerWebSocketContext, CancellationToken, Task> handleWebSocket)
    {
        var uri = new Uri($"http://127.0.0.1:{_port}");
        var server = WebServerBuilder.New(uri).WithWebSockets(handleWebSocket).Build();
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            cts.Cancel();
            await serverTask;
        });
    }
}