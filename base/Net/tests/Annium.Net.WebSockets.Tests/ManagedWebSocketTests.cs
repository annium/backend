using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

public class ManagedWebSocketTests : TestBase
{
    [Fact]
    public async Task Send_Normal()
    {
        // arrange
        await using var _ = RunServer(HandleClientEcho);

        var clientSocket = new System.Net.WebSockets.ClientWebSocket();
        var managedSocket = new ManagedWebSocket(clientSocket);
        var messages = new ConcurrentQueue<string>();
        using var textSubscription = managedSocket.ObserveText().Subscribe(x => messages.Enqueue(Encoding.UTF8.GetString(x.Span)));

        await clientSocket.ConnectAsync(ServerUri, CancellationToken.None);
        var listenTask = managedSocket.ListenAsync(CancellationToken.None);

        // act
        await managedSocket.SendTextAsync("demo"u8.ToArray());

        // assert
        await Expect.To(() => messages.Has(1));
        messages.At(0).Is("demo");

        // teardown
        await clientSocket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
        await listenTask;
    }

    [Fact]
    public async Task Send_Canceled()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Send_SocketNotConnected()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Send_SocketClosed()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Send_SocketAborted()
    {
        await Task.CompletedTask;
    }
}