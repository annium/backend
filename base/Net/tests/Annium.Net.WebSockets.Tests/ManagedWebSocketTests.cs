using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

public class ManagedWebSocketTests : TestBase, IAsyncLifetime
{
    private System.Net.WebSockets.ClientWebSocket _clientSocket = default!;
    private ManagedWebSocket _managedSocket = default!;
    private readonly ConcurrentQueue<string> _texts = new();
    private readonly ConcurrentQueue<byte[]> _binaries = new();

    [Fact]
    public async Task Send_NotConnected()
    {
        // arrange
        const string message = "demo";

        // act
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
    }

    [Fact]
    public async Task Send_Canceled()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // act
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        result.Is(WebSocketSendStatus.Canceled);
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None).GetAwaiter();
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (ctx, _) => await ctx.WebSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, string.Empty, default));
        await ConnectAndStartListenAsync();

        // act
        await Task.Delay(1);
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
    }

    [Fact]
    public async Task Send_ClientAborted()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.Abort();
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
    }

    [Fact]
    public async Task Send_ServerAborted()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer((ctx, _) =>
        {
            ctx.WebSocket.Abort();

            return Task.CompletedTask;
        });
        await ConnectAndStartListenAsync();

        // act
        await Task.Delay(1);
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
    }

    [Fact]
    public async Task Send_Normal()
    {
        // arrange
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

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
    }

    [Fact]
    public async Task Listen_Canceled()
    {
        // arrange
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAsync();

        // act
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        result.Is(WebSocketReceiveStatus.Canceled);
    }

    [Fact]
    public async Task Listen_ClientClosed()
    {
        // arrange
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAsync();
        await _clientSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, default);

        // act
        var result = await ListenAsync();

        // assert
        result.Is(WebSocketReceiveStatus.ClosedLocal);
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        // arrange
        await using var _ = RunServer(async (ctx, _) => await ctx.WebSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, string.Empty, default));
        await ConnectAsync();

        // act
        var result = await ListenAsync();

        // assert
        result.Is(WebSocketReceiveStatus.ClosedRemote);
    }

    [Fact]
    public async Task Listen_ClientAborted()
    {
        // arrange
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAsync();
        var listenTask = ListenAsync();

        // act
        _clientSocket.Abort();
        var result = await listenTask;

        // assert
        result.Is(WebSocketReceiveStatus.ClosedLocal);
    }

    [Fact]
    public async Task Listen_ServerAborted()
    {
        // arrange
        await using var _ = RunServer((ctx, _) =>
        {
            ctx.WebSocket.Abort();

            return Task.CompletedTask;
        });
        await ConnectAsync();
        var listenTask = ListenAsync();

        // act
        await Task.Delay(1);
        var result = await listenTask;

        // assert
        result.Is(WebSocketReceiveStatus.ClosedRemote);
    }

    [Fact]
    public async Task Listen_Normal()
    {
        // arrange
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

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
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        // arrange
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 1_000_000))
            .ToArray();
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

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
        result.Is(WebSocketReceiveStatus.ClosedRemote);
    }

    [Fact]
    public async Task Listen_BothTypes()
    {
        // arrange
        var texts = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        var binaries = texts
            .Select(Encoding.UTF8.GetBytes)
            .ToArray();
        await using var _ = RunServer(async (ctx, ct) =>
        {
            var serverSocket = new ManagedWebSocket(ctx.WebSocket);

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
        result.Is(WebSocketReceiveStatus.ClosedRemote);
    }

    public async Task InitializeAsync()
    {
        _clientSocket = new System.Net.WebSockets.ClientWebSocket();
        _managedSocket = new ManagedWebSocket(_clientSocket);
        _managedSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _managedSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();
    }

    private Task ConnectAsync(CancellationToken ct = default)
    {
        return _clientSocket.ConnectAsync(ServerUri, ct);
    }

    private Task<WebSocketReceiveStatus> ListenAsync(CancellationToken ct = default)
    {
        return _managedSocket.ListenAsync(ct);
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        return await _managedSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        return await _managedSocket.SendBinaryAsync(data, ct);
    }
}