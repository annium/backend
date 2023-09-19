using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;
using Xunit.Abstractions;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Tests.Internal;

public class ManagedWebSocketTests : TestBase, IAsyncLifetime
{
    private NativeClientWebSocket _clientSocket = default!;
    private ManagedWebSocket _managedSocket = default!;
    private readonly ConcurrentQueue<string> _texts = new();
    private readonly ConcurrentQueue<byte[]> _binaries = new();

    public ManagedWebSocketTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Send_NotConnected()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";

        // act
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None).GetAwaiter();
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
        var serverTcs = new TaskCompletionSource();
        await using var _ = RunServerBase(async (_, ctx, _) =>
        {
            await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, default);
            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                .GetAwaiter();
        });
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        await serverTcs.Task;

        // act
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientAborted()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAndStartListenAsync();

        // act
        _clientSocket.Abort();
        var result = await SendTextAsync(message);

        // assert
        result.Is(WebSocketSendStatus.Closed);
        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerAborted()
    {
        // arrange
        this.Trace("start");
        const string message = "demo";
        var serverTcs = new TaskCompletionSource();
        await using var _ = RunServerBase(async (_, ctx, _) =>
        {
            ctx.WebSocket.Abort();

            await Task.CompletedTask;

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                .GetAwaiter();
        });
        await ConnectAndStartListenAsync();

        // act
        // delay to let server close connection
        await serverTcs.Task;

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
        var serverTcs = new TaskCompletionSource();
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

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                .GetAwaiter();

            await serverSocket.ListenAsync(ct);
        });
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        await serverTcs.Task;

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
        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Canceled()
    {
        // arrange
        this.Trace("start");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();

        // act
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientClosed()
    {
        // arrange
        this.Trace("start");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();
        await _clientSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, default);

        // act
        var result = await ListenAsync();

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        // arrange
        this.Trace("start");
        await using var _ = RunServerBase(async (_, ctx, _) => await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, default));
        await ConnectAsync();

        // act
        var result = await ListenAsync();

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientAborted()
    {
        // arrange
        this.Trace("start");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));
        await ConnectAsync();
        var listenTask = ListenAsync();

        // act
        _clientSocket.Abort();
        var result = await listenTask;

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerAborted()
    {
        // arrange
        this.Trace("start");
        var serverTcs = new TaskCompletionSource();
        await using var _ = RunServerBase(async (_, ctx, _) =>
        {
            ctx.WebSocket.Abort();

            await Task.CompletedTask;

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                .GetAwaiter();
        });
        await ConnectAsync();
        var listenTask = ListenAsync();

        // act

        // delay to let server close connection
        await serverTcs.Task;

        var result = await listenTask;

        // assert
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();
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
        this.Trace("done");
    }

    public async Task InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new NativeClientWebSocket();
        _managedSocket = new ManagedWebSocket(_clientSocket, Logger);
        this.Trace<string, string>("created pair of {clientSocket} and {managedSocket}", _clientSocket.GetFullId(), _managedSocket.GetFullId());

        _managedSocket.TextReceived += x => _texts.Enqueue(Encoding.UTF8.GetString(x.Span));
        _managedSocket.BinaryReceived += x => _binaries.Enqueue(x.ToArray());

        await Task.CompletedTask;

        this.Trace("done");
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private IAsyncDisposable RunServer(Func<ManagedWebSocket, CancellationToken, Task> handleWebSocket)
    {
        return RunServerBase(async (sp, ctx, ct) =>
        {
            this.Trace("start");

            var socket = new ManagedWebSocket(ctx.WebSocket, sp.Resolve<ILogger>());

            this.Trace<string>("handle {socket}", socket.GetFullId());
            await handleWebSocket(socket, ct);

            this.Trace("done");
        });
    }

    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();

        this.Trace("done");
    }

    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await _clientSocket.ConnectAsync(ServerUri, ct);

        this.Trace("done");
    }

    private async Task<WebSocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.ListenAsync(ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}