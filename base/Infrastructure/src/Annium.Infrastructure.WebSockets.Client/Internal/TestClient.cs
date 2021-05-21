using System;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Runtime.Time;
using Annium.Net.WebSockets;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class TestClient : ClientBase<WebSocket, NativeWebSocket>, ITestClient
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;

        public TestClient(
            NativeWebSocket socket,
            ITimeProvider timeProvider,
            Serializer serializer,
            ITestClientConfiguration configuration
        ) : base(
            new WebSocket(socket, configuration.WebSocketOptions),
            timeProvider,
            serializer,
            configuration
        )
        {
            Socket.ConnectionLost += () => ConnectionLost.Invoke();
        }

        public override async ValueTask DisposeAsync()
        {
            this.Trace(() => "start");
            await base.DisposeAsync();
            this.Trace(() => "done");
        }
    }
}