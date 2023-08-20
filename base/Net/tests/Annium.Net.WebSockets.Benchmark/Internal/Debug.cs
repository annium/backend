using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Net.Servers;
using Annium.Threading.Tasks;

namespace Annium.Net.WebSockets.Benchmark.Internal;

internal class Debug
{
    public static async Task RunAsync()
    {
        Trace("start");

        await using var executor = Executor.Background.Parallel<Debug>();
        executor.Start();

        var cts = new CancellationTokenSource();

        // server
        var server = WebServerBuilder.New(new Uri("http://127.0.0.1:9898")).WithWebSockets(HandleWebSocket).Build();
        var serverRunTask = Task.Run(() => server.RunAsync(cts.Token), CancellationToken.None);

        // client
        var socket = new System.Net.WebSockets.ClientWebSocket();
        var client = new ManagedWebSocket(socket);
        var messageCount = 1_000_000L;
        var counter = messageCount;
        client.TextReceived += _ => Interlocked.Decrement(ref counter);
        await socket.ConnectAsync(new Uri("ws://127.0.0.1:9898/"), CancellationToken.None);
        var clientListenTask = Task.Run(() => client.ListenAsync(cts.Token));
        await Task.Delay(10);
        Trace("client: send messages");
        ReadOnlyMemory<byte> msg = "demo"u8.ToArray().AsMemory();
        for (var i = 0; i < messageCount; i++)
            await client.SendTextAsync(msg, CancellationToken.None);


        Trace("client: wait for all messages returned");
        await Wait.UntilAsync(() => counter == 0);

        Trace("client: disconnect");
        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

        cts.Cancel();
        await Task.WhenAll(clientListenTask, serverRunTask);

        Trace("done");
    }

    private static async Task HandleWebSocket(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        var clientSocket = new ManagedWebSocket(ctx.WebSocket);

        // create channel to decouple read/write flow
        var channel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>();

        // pass incoming messages to channel writer
        clientSocket.TextReceived += text => channel.Writer.TryWrite(text.ToArray());

        // start echo task
        var echoCts = new CancellationTokenSource();
        var echoTask = Task.Run(() => RunEchoAsync(channel.Reader, clientSocket, echoCts.Token));

        // listen till end
        await clientSocket.ListenAsync(CancellationToken.None);

        // mark writer complete when client is disconnected
        channel.Writer.Complete();

        // cancel and wait for echo task
        echoCts.Cancel();
        await echoTask;
    }

    private static async Task RunEchoAsync(ChannelReader<ReadOnlyMemory<byte>> reader, ManagedWebSocket clientSocket, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var data = await reader.ReadAsync(ct);
                await clientSocket.SendTextAsync(data, ct);
            }

            while (true)
            {
                if (reader.TryRead(out var data))
                {
                    await clientSocket.SendTextAsync(data, ct);
                }
                else
                {
                    break;
                }
            }
        }
        catch (ChannelClosedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static void Trace(string msg)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");
    }
}