using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Net.WebSockets.Internal;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Tests.Internal;

public class ManagedWebSocketTests : TestBase, IAsyncLifetime
{
    private NativeClientWebSocket _clientSocket = default!;
    private ManagedWebSocket _managedSocket = default!;
    private readonly ConcurrentQueue<string> _texts = new();
    private readonly ConcurrentQueue<byte[]> _binaries = new();

    [Fact]
    public async Task Send_NotConnected()
    {
        // arrange
        this.TraceOld("start");
        const string message = "demo";

        // act
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Send_Canceled()
    {
        // arrange
        this.TraceOld("start");
        const string message = "demo";
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        result.Is(WebSocketSendStatus.Canceled);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        // arrange
        this.TraceOld("start");
        const string message = "demo";
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None).GetAwaiter();
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        // arrange
        this.TraceOld("start");
        const string message = "demo";
        await using var _ = RunServerBase(async (ctx, _) => await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, default));
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        await Task.Delay(10);

        // act
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Send_ClientAborted()
    {
        // arrange
        this.TraceOld("start");
        const string message = "demo";
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.Abort();
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Send_ServerAborted()
    {
        // arrange
        this.TraceOld("start");
        const string message = "demo";
        await using var _ = RunServerBase(async (ctx, _) =>
        {
            ctx.WebSocket.Abort();

            await Task.CompletedTask;
        });
        await ConnectAndStartListenAsync();

        // act
        await Task.Delay(1);
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Send_Normal()
    {
        // arrange
        this.TraceOld("start");
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // delay to let server setup subscriptions
        await Task.Delay(10);

        // act
        var textResult = await SendTextAsync(text);
        var binaryResult = await SendBinaryAsync(binary);

        // assert
        textResult.Is(WebSocketSendStatus.Ok);
        binaryResult.Is(WebSocketSendStatus.Ok);
        var expectedTexts = new[] { text };
        var expectedBinaries = new[] { binary };
        await Expect.To(() => _texts.IsEqual(expectedTexts));
        await Expect.To(() => _binaries.IsEqual(expectedBinaries));
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_Canceled()
    {
        // arrange
        this.TraceOld("start");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();

        // act
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_ClientClosed()
    {
        // arrange
        this.TraceOld("start");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();
        await _clientSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, default);

        // act
        var result = await ListenAsync();

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        // arrange
        this.TraceOld("start");
        await using var _ = RunServerBase(async (ctx, _) => await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, default));
        await ConnectAsync();

        // act
        var result = await ListenAsync();

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_ClientAborted()
    {
        // arrange
        this.TraceOld("start");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();
        var listenTask = ListenAsync();

        // act
        _clientSocket.Abort();
        var result = await listenTask;

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_ServerAborted()
    {
        // arrange
        this.TraceOld("start");
        await using var _ = RunServerBase(async (ctx, _) =>
        {
            ctx.WebSocket.Abort();

            await Task.CompletedTask;
        });
        await ConnectAsync();
        var listenTask = ListenAsync();

        // act
        await Task.Delay(1);
        var result = await listenTask;

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_Normal()
    {
        // arrange
        this.TraceOld("start");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message), ct);
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();
        ListenAsync().GetAwaiter();

        // assert
        await Expect.To(() => _texts.IsEqual(messages), 1000);
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        // arrange
        this.TraceOld("start");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 1_000_000))
            .ToArray();
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message), ct);
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();
        var listenTask = ListenAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(messages), 1000);
        var result = await listenTask;
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_BothTypes()
    {
        // arrange
        this.TraceOld("start");
        var texts = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        var binaries = texts
            .Select(Encoding.UTF8.GetBytes)
            .ToArray();
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            foreach (var message in texts)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message), ct);
                await Task.Delay(1, CancellationToken.None);
            }

            foreach (var message in binaries)
            {
                await serverSocket.SendBinaryAsync(message, ct);
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();
        var listenTask = ListenAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(texts), 1000);
        await Expect.To(() => _binaries.IsEqual(binaries), 1000);
        var result = await listenTask;
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    public async Task InitializeAsync()
    {
        this.TraceOld("start");

        _clientSocket = new NativeClientWebSocket();
        _managedSocket = new ManagedWebSocket(_clientSocket);
        this.TraceOld($"created pair of {_clientSocket.GetFullId()} and {_managedSocket.GetFullId()}");

        _managedSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _managedSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        await Task.CompletedTask;

        this.TraceOld("done");
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private IAsyncDisposable RunServer(Func<ManagedWebSocket, CancellationToken, Task> handleWebSocket)
    {
        return RunServerBase(async (ctx, ct) =>
        {
            this.TraceOld("start");

            var socket = new ManagedWebSocket(ctx.WebSocket);

            this.TraceOld($"handle {socket.GetFullId()}");
            await handleWebSocket(socket, ct);

            this.TraceOld("done");
        });
    }

    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        this.TraceOld("start");

        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();

        this.TraceOld("done");
    }

    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.TraceOld("start");

        await _clientSocket.ConnectAsync(ServerUri, ct);

        this.TraceOld("done");
    }

    private async Task<WebSocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.TraceOld("start");

        var result = await _managedSocket.ListenAsync(ct);

        this.TraceOld("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.TraceOld("start");

        var result = await _managedSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

        this.TraceOld("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        this.TraceOld("start");

        var result = await _managedSocket.SendBinaryAsync(data, ct);

        this.TraceOld("done");

        return result;
    }
}