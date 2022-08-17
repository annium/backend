using System;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class TestClient : ClientBase<WebSocket>, ITestClient, ILogSubject<TestClient>
{
    public new ILogger<TestClient> Logger { get; }
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;

    public TestClient(
        NativeWebSocket socket,
        ITimeProvider timeProvider,
        Serializer serializer,
        ITestClientConfiguration configuration,
        ILoggerFactory loggerFactory
    ) : base(
        new WebSocket(socket, configuration.WebSocketOptions, loggerFactory),
        timeProvider,
        serializer,
        configuration,
        loggerFactory.Get<TestClient>()
    )
    {
        Logger = loggerFactory.Get<TestClient>();
        Socket.ConnectionLost += () => ConnectionLost.Invoke();
    }

    public override async ValueTask DisposeAsync()
    {
        this.Log().Trace("start");
        await base.DisposeAsync();
        this.Log().Trace("disconnect socket");
        await Socket.DisconnectAsync();
        this.Log().Trace("dispose socket");
        await Socket.DisposeAsync();
        this.Log().Trace("done");
    }
}