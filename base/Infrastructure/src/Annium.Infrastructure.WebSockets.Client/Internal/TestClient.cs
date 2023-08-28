using System;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class TestClient : ClientBase<WebSocket>, ITestClient, ILogSubject
{
    public new ILogger Logger { get; }
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;

    public TestClient(
        NativeWebSocket socket,
        ITimeProvider timeProvider,
        Serializer serializer,
        ITestClientConfiguration configuration,
        ILoggerFactory loggerFactory,
        ITracer tracer
    ) : base(
        new WebSocket(socket, configuration.WebSocketOptions, tracer),
        timeProvider,
        serializer,
        configuration,
        loggerFactory.Get<TestClient>(),
        tracer
    )
    {
        Logger = loggerFactory.Get<TestClient>();
        Socket.ConnectionLost += () => ConnectionLost.Invoke();
    }

    public override async ValueTask DisposeAsync()
    {
        this.Trace("start");
        await base.DisposeAsync();
        this.Trace("disconnect socket");
        await Socket.DisconnectAsync();
        this.Trace("dispose socket");
        await Socket.DisposeAsync();
        this.Trace("done");
    }
}