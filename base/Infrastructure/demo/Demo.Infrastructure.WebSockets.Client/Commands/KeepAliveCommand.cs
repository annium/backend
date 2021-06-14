using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class KeepAliveCommand : AsyncCommand<ServerCommandConfiguration>, ILogSubject
    {
        public override string Id { get; } = "keep-alive";
        public override string Description => $"test {Id} flow";
        public ILogger Logger { get; }
        private readonly IClientFactory _clientFactory;

        public KeepAliveCommand(
            IClientFactory clientFactory,
            ILogger<RequestCommand> logger
        )
        {
            _clientFactory = clientFactory;
            Logger = logger;
        }

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken ct)
        {
            var configuration = new ClientConfiguration().ConnectTo(cfg.Server).WithActiveKeepAlive(1);
            var client = _clientFactory.Create(configuration);
            client.ConnectionLost += () =>
            {
                this.Debug("connection lost");
                return Task.CompletedTask;
            };
            client.ConnectionRestored += () =>
            {
                this.Debug("connection restored");
                return Task.CompletedTask;
            };

            this.Debug($"Connecting to {cfg.Server}");
            await client.ConnectAsync(ct);
            this.Debug($"Connected to {cfg.Server}");

            await ct;

            if (client.IsConnected)
            {
                this.Debug("Disconnecting");
                await client.DisconnectAsync();
            }

            this.Debug("Disconnected");
        }
    }
}