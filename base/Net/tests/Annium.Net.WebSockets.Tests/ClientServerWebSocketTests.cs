using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.WebSockets.Tests;

public class ClientServerWebSocketTests : TestBase, IAsyncLifetime
{
    private ClientWebSocket _clientSocket = default!;
    private readonly ConcurrentQueue<string> _texts = new();
    private readonly ConcurrentQueue<byte[]> _binaries = new();

    public ClientServerWebSocketTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Send_NotConnected()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.Trace("done");
    }

    [Fact]
    public async Task Send_Canceled()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";
        await using var _ = RunServer(async serverSocket => await serverSocket.WhenDisconnected());

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        result.Is(WebSocketSendStatus.Canceled);
        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";
        await using var _ = RunServer(async serverSocket => await serverSocket.WhenDisconnected());

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("disconnect");
        _clientSocket.Disconnect();

        this.Trace("send text");
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";
        await using var _ = RunServer(serverSocket =>
        {
            serverSocket.Disconnect();

            return Task.CompletedTask;
        });

        this.Trace("connect");
        await ConnectAsync();

        // delay to let server close connection
        await Task.Delay(20);

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.Trace("done");
    }

    // [Fact]
    // public async Task Send_Normal()
    // {
    //     // arrange
    //     this.Trace("start");
    //     const string text = "demo";
    //     var binary = Encoding.UTF8.GetBytes(text);
    //     await using var _ = RunServer(async serverSocket =>
    //     {
    //         serverSocket.TextReceived += x => serverSocket
    //             .SendTextAsync(x.ToArray(), CancellationToken.None)
    //             .GetAwaiter()
    //             .GetResult();
    //         this.Trace("server subscribed to text");
    //
    //         serverSocket.BinaryReceived += x => serverSocket
    //             .SendBinaryAsync(x.ToArray(), CancellationToken.None)
    //             .GetAwaiter()
    //             .GetResult();
    //         this.Trace("server subscribed to binary");
    //
    //         await serverSocket.WhenDisconnected();
    //         this.Trace("server socket closed");
    //     });
    //     await Connect();
    //
    //     // delay to let server setup subscriptions
    //     await Task.Delay(10);
    //
    //     // act
    //     var textResult = await SendTextAsync(text);
    //     var binaryResult = await SendBinaryAsync(binary);
    //
    //     // assert
    //     textResult.Is(WebSocketSendStatus.Ok);
    //     binaryResult.Is(WebSocketSendStatus.Ok);
    //     var expectedTexts = new[] { text };
    //     var expectedBinaries = new[] { binary };
    //     await Expect.To(() => _texts.IsEqual(expectedTexts));
    //     await Expect.To(() => _binaries.IsEqual(expectedBinaries));
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Send_Reconnect()
    // {
    //     // arrange
    //     this.Trace("start");
    //     const string text = "demo";
    //     var binary = Encoding.UTF8.GetBytes(text);
    //     await using var _ = RunServer(async serverSocket =>
    //     {
    //         serverSocket.TextReceived += x => serverSocket
    //             .SendTextAsync(x.ToArray(), CancellationToken.None)
    //             .GetAwaiter()
    //             .GetResult();
    //         this.Trace("server subscribed to text");
    //
    //         serverSocket.BinaryReceived += x => serverSocket
    //             .SendBinaryAsync(x.ToArray(), CancellationToken.None)
    //             .GetAwaiter()
    //             .GetResult();
    //         this.Trace("server subscribed to binary");
    //
    //         await serverSocket.WhenDisconnected();
    //         this.Trace("server socket closed");
    //     });
    //
    //     // act - send text
    //     await Connect();
    //     await Task.Delay(10);
    //     var textResult = await SendTextAsync(text);
    //     textResult.Is(WebSocketSendStatus.Ok);
    //     var expectedTexts = new[] { text };
    //     await Expect.To(() => _texts.IsEqual(expectedTexts));
    //     await _clientSocket.DisconnectAsync();
    //
    //     // act - send binary
    //     await Connect();
    //     await Task.Delay(10);
    //     var binaryResult = await SendBinaryAsync(binary);
    //     binaryResult.Is(WebSocketSendStatus.Ok);
    //     var expectedBinaries = new[] { binary };
    //     await Expect.To(() => _binaries.IsEqual(expectedBinaries));
    //     await _clientSocket.DisconnectAsync();
    //
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Listen_Canceled()
    // {
    //     // arrange
    //     this.Trace("start");
    //     await using var _ = RunServer(async serverSocket =>
    //     {
    //         await serverSocket.WhenDisconnected();
    //         await Task.Delay(100);
    //     });
    //     var cts = new CancellationTokenSource();
    //     await Connect(cts.Token);
    //     cts.Cancel();
    //
    //     // act
    //     var result = await _clientSocket.IsClosed;
    //
    //     // assert
    //     result.Status.Is(WebSocketCloseStatus.ClosedLocal);
    //     result.Exception.IsDefault();
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Listen_ClientClosed()
    // {
    //     // arrange
    //     this.Trace("start");
    //     await using var _ = RunServer(async serverSocket => await serverSocket.WhenDisconnected());
    //     await Connect();
    //     await _clientSocket.DisconnectAsync();
    //
    //     // act
    //     var result = await _clientSocket.IsClosed;
    //
    //     // assert
    //     result.Status.Is(WebSocketCloseStatus.ClosedLocal);
    //     result.Exception.IsDefault();
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Listen_ServerClosed()
    // {
    //     // arrange
    //     this.Trace("start");
    //     await using var _ = RunServer(serverSocket =>
    //     {
    //         serverSocket.Disconnect();
    //
    //         return Task.CompletedTask;
    //     });
    //     await Connect();
    //
    //     // act
    //     var result = await _clientSocket.IsClosed;
    //
    //     // assert
    //     result.Status.Is(WebSocketCloseStatus.ClosedRemote);
    //     result.Exception.IsDefault();
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Listen_Normal()
    // {
    //     // arrange
    //     this.Trace("start");
    //     var messages = Enumerable.Range(0, 3)
    //         .Select(x => new string((char)x, 10))
    //         .ToArray();
    //     await using var _ = RunServer(async serverSocket =>
    //     {
    //         foreach (var message in messages)
    //         {
    //             await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
    //             await Task.Delay(1, CancellationToken.None);
    //         }
    //     });
    //
    //     // act
    //     await Connect();
    //
    //     // assert
    //     await Expect.To(() => _texts.IsEqual(messages), 1000);
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Listen_SmallBuffer()
    // {
    //     // arrange
    //     this.Trace("start");
    //     var messages = Enumerable.Range(0, 3)
    //         .Select(x => new string((char)x, 1_000_000))
    //         .ToArray();
    //     await using var _ = RunServer(async serverSocket =>
    //     {
    //         foreach (var message in messages)
    //         {
    //             await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
    //             await Task.Delay(1, CancellationToken.None);
    //         }
    //     });
    //
    //     // act
    //     await Connect();
    //
    //     // assert
    //     await Expect.To(() => _texts.IsEqual(messages), 1000);
    //     var result = await _clientSocket.IsClosed;
    //     result.Status.Is(WebSocketCloseStatus.ClosedRemote);
    //     result.Exception.IsDefault();
    //     this.Trace("done");
    // }
    //
    // [Fact]
    // public async Task Listen_BothTypes()
    // {
    //     // arrange
    //     this.Trace("start");
    //     var texts = Enumerable.Range(0, 3)
    //         .Select(x => new string((char)x, 10))
    //         .ToArray();
    //     var binaries = texts
    //         .Select(Encoding.UTF8.GetBytes)
    //         .ToArray();
    //     await using var _ = RunServer(async serverSocket =>
    //     {
    //         foreach (var message in texts)
    //         {
    //             await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
    //             await Task.Delay(1, CancellationToken.None);
    //         }
    //
    //         foreach (var message in binaries)
    //         {
    //             await serverSocket.SendBinaryAsync(message);
    //             await Task.Delay(1, CancellationToken.None);
    //         }
    //     });
    //
    //     // act
    //     await Connect();
    //
    //     // assert
    //     await Expect.To(() => _texts.IsEqual(texts), 1000);
    //     await Expect.To(() => _binaries.IsEqual(binaries), 1000);
    //     var result = await _clientSocket.IsClosed;
    //     result.Status.Is(WebSocketCloseStatus.ClosedRemote);
    //     result.Exception.IsDefault();
    //     this.Trace("done");
    // }

    public Task InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new ClientWebSocket(ClientWebSocketOptions.Default with { ReconnectDelay = 1 }, Logger);
        _clientSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _clientSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        _clientSocket.OnConnected += () => Console.WriteLine("STATE: Connected");
        _clientSocket.OnDisconnected += x => Console.WriteLine($"STATE: Disconnected: {x}");
        _clientSocket.OnError += x => Console.WriteLine($"STATE: Error: {x}");

        this.Trace("done");

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        this.Trace("start");

        _clientSocket.Disconnect();

        this.Trace("done");

        return Task.CompletedTask;
    }

    private IAsyncDisposable RunServer(Func<ServerWebSocket, Task> handleWebSocket)
    {
        return RunServerBase(async (ctx, logger, ct) =>
        {
            this.Trace("start");

            var socket = new ServerWebSocket(ctx.WebSocket, logger, ct);

            this.Trace<string>("handle {socket}", socket.GetFullId());
            await handleWebSocket(socket);

            this.Trace<string>("disconnect {socket}", socket.GetFullId());
            socket.Disconnect();

            this.Trace("done");
        });
    }

    private async Task ConnectAsync()
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        _clientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());
        _clientSocket.OnConnected += () =>
        {
            _clientSocket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult();
        };

        _clientSocket.Connect(ServerUri);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        this.Trace("done");
    }


    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _clientSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _clientSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}

internal static class WebSocketExtensions
{
    public static Task WhenDisconnected(this ServerWebSocket socket)
    {
        var tcs = new TaskCompletionSource();

        socket.Trace<string>("subscribe {tcs} to OnDisconnected", tcs.GetFullId());
        socket.OnDisconnected += status =>
        {
            socket.Trace("set {tcs} to signaled state for status {status}", tcs.GetFullId(), status);
            tcs.SetResult();
        };

        return tcs.Task;
    }
}