using System;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class TestClient : ClientBase<ServerWebSocket>, ITestClient
{
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;

    public TestClient(
        NativeWebSocket socket,
        ITimeProvider timeProvider,
        Serializer serializer,
        ITestClientConfiguration configuration,
        ILogger logger
    ) : base(
        new ServerWebSocket(socket, configuration.WebSocketOptions, logger),
        timeProvider,
        serializer,
        configuration,
        logger
    )
    {
        Socket.OnDisconnected += _ => ConnectionLost.Invoke();
    }

    public override async ValueTask DisposeAsync()
    {
        this.Trace("start");
        await base.DisposeAsync();
        this.Trace("disconnect socket");
        Socket.Disconnect();
        this.Trace("done");
    }
}