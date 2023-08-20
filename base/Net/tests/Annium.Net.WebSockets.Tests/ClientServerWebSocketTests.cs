using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

public class ClientServerWebSocketTests : TestBase, IAsyncLifetime
{
    private ClientWebSocket _clientSocket = default!;
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.DisconnectAsync().GetAwaiter();
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.DisconnectAsync());
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();
        await _clientSocket.DisconnectAsync();

        // act
        var result = await ListenAsync();

        // assert
        result.Is(WebSocketReceiveStatus.ClosedLocal);
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        // arrange
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.DisconnectAsync());
        await ConnectAsync();

        // act
        var result = await ListenAsync();

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
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        // arrange
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
        result.Is(WebSocketReceiveStatus.ClosedRemote);
    }

    public async Task InitializeAsync()
    {
        _clientSocket = new ClientWebSocket();
        _clientSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _clientSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private IAsyncDisposable RunServer(Func<IServerWebSocket, CancellationToken, Task> handleWebSocket)
    {
        return RunServerBase((ctx, ct) => handleWebSocket(new ServerWebSocket(ctx.WebSocket), ct));
    }

    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();
    }

    private ValueTask ConnectAsync(CancellationToken ct = default)
    {
        return _clientSocket.ConnectAsync(ServerUri, ct);
    }

    private ValueTask<WebSocketReceiveStatus> ListenAsync(CancellationToken ct = default)
    {
        return _clientSocket.ListenAsync(ct);
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        return await _clientSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        return await _clientSocket.SendBinaryAsync(data, ct);
    }
}