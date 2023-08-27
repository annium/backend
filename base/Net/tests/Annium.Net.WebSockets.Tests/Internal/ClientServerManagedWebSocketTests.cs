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

namespace Annium.Net.WebSockets.Tests.Internal;

public class ClientServerManagedWebSocketTests : TestBase, IAsyncLifetime
{
    private IClientManagedWebSocket _clientSocket = default!;
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
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);
        await ConnectAsync();

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
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);
        await ConnectAsync();

        // act
        _clientSocket.DisconnectAsync().GetAwaiter();
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
        await using var _ = RunServer(async serverSocket => await serverSocket.DisconnectAsync());
        await ConnectAsync();

        // delay to let server close connection
        await Task.Delay(10);

        // act
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
        await using var _ = RunServer(async serverSocket =>
        {
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.TraceOld("server subscribed to text");

            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.TraceOld("server subscribed to binary");

            await serverSocket.IsClosed;
            this.TraceOld($"server socket closed");
        });
        await ConnectAsync();

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
    public async Task Send_Reconnect()
    {
        // arrange
        this.TraceOld("start");
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        await using var _ = RunServer(async serverSocket =>
        {
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.TraceOld("server subscribed to text");

            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.TraceOld("server subscribed to binary");

            await serverSocket.IsClosed;
            this.TraceOld($"server socket closed");
        });

        // act - send text
        await ConnectAsync();
        await Task.Delay(10);
        var textResult = await SendTextAsync(text);
        textResult.Is(WebSocketSendStatus.Ok);
        var expectedTexts = new[] { text };
        await Expect.To(() => _texts.IsEqual(expectedTexts));
        await _clientSocket.DisconnectAsync();

        // act - send binary
        await ConnectAsync();
        await Task.Delay(10);
        var binaryResult = await SendBinaryAsync(binary);
        binaryResult.Is(WebSocketSendStatus.Ok);
        var expectedBinaries = new[] { binary };
        await Expect.To(() => _binaries.IsEqual(expectedBinaries));
        await _clientSocket.DisconnectAsync();

        this.TraceOld("done");
    }

    [Fact]
    public async Task Listen_Canceled()
    {
        // arrange
        this.TraceOld("start");
        await using var _ = RunServer(async serverSocket =>
        {
            await serverSocket.IsClosed;
            await Task.Delay(100);
        });
        var cts = new CancellationTokenSource();
        await ConnectAsync(cts.Token);
        cts.Cancel();

        // act
        var result = await _clientSocket.IsClosed;

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
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);
        await ConnectAsync();
        await _clientSocket.DisconnectAsync();

        // act
        var result = await _clientSocket.IsClosed;

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
        await using var _ = RunServer(async serverSocket => await serverSocket.DisconnectAsync());
        await ConnectAsync();

        // act
        var result = await _clientSocket.IsClosed;

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
        await using var _ = RunServer(async serverSocket =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();

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
        await using var _ = RunServer(async serverSocket =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(messages), 1000);
        var result = await _clientSocket.IsClosed;
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
        await using var _ = RunServer(async serverSocket =>
        {
            foreach (var message in texts)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            foreach (var message in binaries)
            {
                await serverSocket.SendBinaryAsync(message);
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(texts), 1000);
        await Expect.To(() => _binaries.IsEqual(binaries), 1000);
        var result = await _clientSocket.IsClosed;
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
        this.TraceOld("done");
    }

    public async Task InitializeAsync()
    {
        this.TraceOld("start");

        _clientSocket = new ClientManagedWebSocket();
        _clientSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _clientSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        await Task.CompletedTask;

        this.TraceOld("done");
    }

    public async Task DisposeAsync()
    {
        this.TraceOld("start");

        await _clientSocket.DisconnectAsync();

        this.TraceOld("done");
    }

    private IAsyncDisposable RunServer(Func<IServerManagedWebSocket, Task> handleWebSocket)
    {
        return RunServerBase(async (ctx, ct) =>
        {
            this.TraceOld("start");

            var socket = new ServerManagedWebSocket(ctx.WebSocket, ct);

            this.TraceOld($"handle {socket.GetFullId()}");
            await handleWebSocket(socket);

            this.TraceOld($"disconnect {socket.GetFullId()}");
            await socket.DisconnectAsync();

            this.TraceOld("done");
        });
    }

    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.TraceOld("start");

        await _clientSocket.ConnectAsync(ServerUri, ct);

        this.TraceOld("done");
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.TraceOld("start");

        var result = await _clientSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

        this.TraceOld("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        this.TraceOld("start");

        var result = await _clientSocket.SendBinaryAsync(data, ct);

        this.TraceOld("done");

        return result;
    }
}