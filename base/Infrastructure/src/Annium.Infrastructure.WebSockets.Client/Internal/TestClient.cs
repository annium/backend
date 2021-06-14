using System;
using System.Threading.Tasks;
using Annium.Core.Runtime.Time;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class TestClient : ClientBase<WebSocket>, ITestClient
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;

        public TestClient(
            NativeWebSocket socket,
            ITimeProvider timeProvider,
            Serializer serializer,
            ITestClientConfiguration configuration,
            ILoggerFactory loggerFactory
        ) : base(
            new WebSocket(socket, configuration.WebSocketOptions, loggerFactory.GetLogger<WebSocket>()),
            timeProvider,
            serializer,
            configuration,
            loggerFactory.GetLogger<TestClient>()
        )
        {
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
}