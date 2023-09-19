using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Assertions;
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
        var serverConnectionTcs = new TaskCompletionSource();
        await using var _ = RunServer(async serverSocket =>
        {
            var disconnectionTask = serverSocket.WhenDisconnected();
            serverConnectionTcs.SetResult();
            await disconnectionTask;
        });

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act
        this.Trace("disconnect");
        await DisconnectAsync();

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

        this.Trace("await until disconnected");
        await _clientSocket.WhenDisconnected();

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.Trace("done");
    }

    [Fact]
    public async Task Send_Normal()
    {
        // arrange
        this.Trace("start");
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        var serverConnectionTcs = new TaskCompletionSource();
        await using var _ = RunServer(async serverSocket =>
        {
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to text");

            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to binary");

            var disconnectionTask = serverSocket.WhenDisconnected();
            serverConnectionTcs.TrySetResult();
            await disconnectionTask;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act
        this.Trace("send text");
        var textResult = await SendTextAsync(text);

        this.Trace("send binary");
        var binaryResult = await SendBinaryAsync(binary);

        // assert
        textResult.Is(WebSocketSendStatus.Ok);
        binaryResult.Is(WebSocketSendStatus.Ok);
        var expectedTexts = new[] { text };
        var expectedBinaries = new[] { binary };
        await Expect.To(() => _texts.IsEqual(expectedTexts));
        await Expect.To(() => _binaries.IsEqual(expectedBinaries));
        this.Trace("done");
    }

    [Fact]
    public async Task Send_Reconnect()
    {
        // arrange
        this.Trace("start");
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        var serverConnectionTcs = new TaskCompletionSource();
        await using var _ = RunServer(async serverSocket =>
        {
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to text");

            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to binary");

            var disconnectionTask = serverSocket.WhenDisconnected();
            serverConnectionTcs.TrySetResult();
            await disconnectionTask;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send text");
        var textResult = await SendTextAsync(text);
        textResult.Is(WebSocketSendStatus.Ok);
        var expectedTexts = new[] { text };
        await Expect.To(() => _texts.IsEqual(expectedTexts));

        this.Trace("disconnect");
        await DisconnectAsync();

        // act - send binary
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send binary");
        var binaryResult = await SendBinaryAsync(binary);
        binaryResult.Is(WebSocketSendStatus.Ok);
        var expectedBinaries = new[] { binary };
        await Expect.To(() => _binaries.IsEqual(expectedBinaries));

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Normal()
    {
        // arrange
        this.Trace("start");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        var serverStopTcs = new TaskCompletionSource();
        await using var _ = RunServer(async serverSocket =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            await serverStopTcs.Task;
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(messages), 1000);
        serverStopTcs.SetResult();
        this.Trace("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        // arrange
        this.Trace("start");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 1_000_000))
            .ToArray();
        var serverStopTcs = new TaskCompletionSource();
        await using var _ = RunServer(async serverSocket =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            await serverStopTcs.Task;
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(messages), 1000);
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        var result = await _clientSocket.WhenDisconnected();
        result.Is(WebSocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_BothTypes()
    {
        // arrange
        this.Trace("start");
        var texts = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        var binaries = texts
            .Select(Encoding.UTF8.GetBytes)
            .ToArray();
        var serverStopTcs = new TaskCompletionSource();
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

            await serverStopTcs.Task;
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        await Expect.To(() => _texts.IsEqual(texts), 1000);
        await Expect.To(() => _binaries.IsEqual(binaries), 1000);
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        var result = await _clientSocket.WhenDisconnected();
        result.Is(WebSocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Reconnect()
    {
        // arrange
        this.Trace("start");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        var serverStopTcs = new TaskCompletionSource();
        var connectionIndex = 0;
        var connectionsCount = 3;
        await using var _ = RunServer(async serverSocket =>
        {
            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            // await until 3-rd connection is handled
            if (++connectionIndex == connectionsCount)
            {
                this.Trace($"finally, {connectionIndex}/{connectionsCount} - wait for signal");
                await serverStopTcs.Task;
            }
            else
                this.Trace($"disconnect, {connectionIndex}/{connectionsCount}");
        });

        this.Trace("connect");
        await ConnectAsync();

        // assert
        var expectedMessages = Enumerable.Range(0, connectionsCount)
            .SelectMany(_ => messages)
            .ToArray();
        this.Trace("wait for {messagesCount} messages", expectedMessages.Length);
        await Expect.To(() => _texts.IsEqual(expectedMessages), 1000);
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    public Task InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new ClientWebSocket(ClientWebSocketOptions.Default with { ReconnectDelay = 1 }, Logger);
        _clientSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _clientSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        _clientSocket.OnConnected += () => this.Trace("STATE: Connected");
        _clientSocket.OnDisconnected += status => this.Trace("STATE: Disconnected: {status}", status);
        _clientSocket.OnError += e => Assert.Fail($"Exception occured: {e}");

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
        return RunServerBase(async (sp, ctx, ct) =>
        {
            this.Trace("start");

            var socket = new ServerWebSocket(ctx.WebSocket, sp.Resolve<ILogger>(), ct);

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

        void HandleConnected()
        {
            _clientSocket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult();
            _clientSocket.OnConnected -= HandleConnected;
        }

        _clientSocket.OnConnected += HandleConnected;

        _clientSocket.Connect(ServerUri);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        this.Trace("done");
    }

    private async Task DisconnectAsync()
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        _clientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleDisconnected(WebSocketCloseStatus status)
        {
            _clientSocket.Trace("set {tcs} to signaled state with status {status}", tcs.GetFullId(), status);
            tcs.SetResult();
            _clientSocket.OnDisconnected -= HandleDisconnected;
        }

        _clientSocket.OnDisconnected += HandleDisconnected;

        _clientSocket.Disconnect();

        await tcs.Task;

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

file static class WebSocketExtensions
{
    public static Task<WebSocketCloseStatus> WhenDisconnected(this ClientWebSocket socket)
    {
        var tcs = new TaskCompletionSource<WebSocketCloseStatus>();

        socket.Trace<string>("subscribe {tcs} to OnDisconnected", tcs.GetFullId());

        void HandleDisconnected(WebSocketCloseStatus status)
        {
            socket.Trace("set {tcs} to signaled state with status {status}", tcs.GetFullId(), status);
            tcs.SetResult(status);
            socket.OnDisconnected -= HandleDisconnected;
        }

        socket.OnDisconnected += HandleDisconnected;

        return tcs.Task;
    }

    public static Task<WebSocketCloseStatus> WhenDisconnected(this ServerWebSocket socket)
    {
        var tcs = new TaskCompletionSource<WebSocketCloseStatus>();

        socket.Trace<string>("subscribe {tcs} to OnDisconnected", tcs.GetFullId());

        void HandleDisconnected(WebSocketCloseStatus status)
        {
            socket.Trace("set {tcs} to signaled state with status {status}", tcs.GetFullId(), status);
            tcs.SetResult(status);
            socket.OnDisconnected -= HandleDisconnected;
        }

        socket.OnDisconnected += HandleDisconnected;

        return tcs.Task;
    }
}