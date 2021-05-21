using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Runtime.Time;
using Annium.Net.WebSockets;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class Client : ClientBase<ClientWebSocket, NativeClientWebSocket>, IClient
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly IClientConfiguration _configuration;

        public Client(
            ITimeProvider timeProvider,
            Serializer serializer,
            IClientConfiguration configuration
        ) : base(
            new ClientWebSocket(configuration.WebSocketOptions),
            timeProvider,
            serializer,
            configuration
        )
        {
            _configuration = configuration;
            Socket.ConnectionLost += () => ConnectionLost.Invoke();
            Socket.ConnectionRestored += () => ConnectionRestored.Invoke();
        }

        public Task ConnectAsync(CancellationToken ct = default) =>
            Socket.ConnectAsync(_configuration.Uri, ct);

        public Task DisconnectAsync(CancellationToken ct = default) =>
            Socket.DisconnectAsync(ct);


        public override async ValueTask DisposeAsync()
        {
            this.Trace(() => "start");
            await base.DisposeAsync();
            this.Trace(() => "disconnect socket");
            await Socket.DisconnectAsync(CancellationToken.None);
            this.Trace(() => "dispose socket");
            await Socket.DisposeAsync();
            this.Trace(() => "done");
        }
    }
}