using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading;

namespace Annium.Net.WebSockets.Tests;

public abstract class TestBase
{
    protected readonly Uri ServerUri = new Uri($"ws://127.0.0.1:{new Random().Next(32768, 65536)}");

    protected IAsyncDisposable RunServer(Func<WebSocket, CancellationToken, Task> handleClient)
    {
        var server = new WebSocketServer(new IPEndPoint(IPAddress.Loopback, ServerUri.Port), "/", handleClient);
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            cts.Cancel();
            await serverTask;
        });
    }

    protected static async Task HandleClientEcho(WebSocket rawSocket, CancellationToken ct)
    {
        var socket = new ManagedWebSocket(rawSocket);

        using var binarySubscription = socket.ObserveBinary()
            .DoSequentialAsync(async x =>
            {
                var message = x.ToArray();
                await socket.SendBinaryAsync(message, CancellationToken.None);
            })
            .Subscribe();

        using var textSubscription = socket.ObserveText()
            .DoSequentialAsync(async x =>
            {
                var message = x.ToArray();
                await socket.SendTextAsync(message, CancellationToken.None);
            })
            .Subscribe();

        var status = await socket.ListenAsync(ct);

        await rawSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }
}