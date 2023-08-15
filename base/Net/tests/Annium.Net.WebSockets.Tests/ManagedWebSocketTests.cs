using System.Collections.Concurrent;
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
    private readonly ConcurrentQueue<string> _messages = new();

    [Fact]
    public async Task Send_Normal()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (rawSocket, ct) =>
        {
            var serverSocket = new ManagedWebSocket(rawSocket);

            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // act
        await SendTextAsync(message);

        // assert
        await Expect.To(() => _messages.Has(1));
        _messages.At(0).Is(message);
    }

    [Fact]
    public async Task Send_Equality()
    {
        // arrange
        const string message = "demo";
        var serverMessage = string.Empty;
        await using var _ = RunServer(async (rawSocket, ct) =>
        {
            var serverSocket = new ManagedWebSocket(rawSocket);

            serverSocket.TextReceived += data => serverMessage = Encoding.UTF8.GetString(data.ToArray());

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // act
        await SendTextAsync(message);

        // assert
        await Expect.To(() => serverMessage.Is(message));
    }

    [Fact]
    public async Task Send_Canceled()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (rawSocket, ct) =>
        {
            var serverSocket = new ManagedWebSocket(rawSocket);

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // act
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        result.Is(WebSocketSendStatus.Canceled);
    }

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
    public async Task Send_ClientClosed()
    {
        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (rawSocket, ct) =>
        {
            var serverSocket = new ManagedWebSocket(rawSocket);

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
        await using var _ = RunServer(async (rawSocket, _) => await rawSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, string.Empty, default));
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
        await using var _ = RunServer(async (rawSocket, ct) =>
        {
            var serverSocket = new ManagedWebSocket(rawSocket);

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
        await using var _ = RunServer((rawSocket, _) =>
        {
            rawSocket.Abort();

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
    public async Task Listen_Delayed()
    {
        // arrange
        await using var _ = RunServer(async (rawSocket, ct) =>
        {
            var serverSocket = new ManagedWebSocket(rawSocket);

            for (var i = 0; i < 3; i++)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes($"x{i}"), ct);
                await Task.Delay(1, CancellationToken.None);
            }
        });

        // act
        await ConnectAsync();
        await Task.Delay(100);
        ListenAsync().GetAwaiter();

        // assert
        await Expect.To(() => _messages.Has(3), 1000);
    }

    public async Task InitializeAsync()
    {
        _clientSocket = new System.Net.WebSockets.ClientWebSocket();
        _managedSocket = new ManagedWebSocket(_clientSocket);
        _managedSocket.TextReceived += x => _messages.Enqueue(Encoding.UTF8.GetString(x.Span));

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

    private Task ListenAsync(CancellationToken ct = default)
    {
        return _managedSocket.ListenAsync(ct);
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        return await _managedSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);
    }
}