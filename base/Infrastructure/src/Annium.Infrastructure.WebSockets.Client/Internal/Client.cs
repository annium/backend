using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Runtime.Time;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class Client : ClientBase<ClientWebSocket>, IClient
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly IClientConfiguration _configuration;

        public Client(
            ITimeProvider timeProvider,
            Serializer serializer,
            IClientConfiguration configuration,
            ILoggerFactory loggerFactory
        ) : base(
            new ClientWebSocket(configuration.WebSocketOptions, loggerFactory.GetLogger<ClientWebSocket>()),
            timeProvider,
            serializer,
            configuration,
            loggerFactory.GetLogger<Client>()
        )
        {
            _configuration = configuration;
            Socket.ConnectionLost += () => ConnectionLost.Invoke();
            Socket.ConnectionRestored += () => ConnectionRestored.Invoke();
        }

        public Task ConnectAsync(CancellationToken ct = default) =>
            Socket.ConnectAsync(_configuration.Uri, ct);

        public Task DisconnectAsync() =>
            Socket.DisconnectAsync();


        public override async ValueTask DisposeAsync()
        {
            Logger.Trace("start");
            await base.DisposeAsync();
            Logger.Trace("disconnect socket");
            await Socket.DisconnectAsync();
            Logger.Trace("dispose socket");
            await Socket.DisposeAsync();
            Logger.Trace("done");
        }
    }
}