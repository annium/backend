using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class KeepAliveCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly IClientFactory _clientFactory;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "keep-alive";
        public override string Description => $"test {Id} flow";

        public KeepAliveCommand(
            IClientFactory clientFactory,
            ILogger<RequestCommand> logger
        )
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken token)
        {
            var configuration = new ClientConfiguration().ConnectTo(cfg.Server).WithActiveKeepAlive(5, 5);
            var client = _clientFactory.Create(configuration);
            client.ConnectionLost += () =>
            {
                _logger.Debug("connection lost");
                return Task.CompletedTask;
            };
            client.ConnectionRestored += () =>
            {
                _logger.Debug("connection restored");
                return Task.CompletedTask;
            };

            _logger.Debug($"Connecting to {cfg.Server}");
            await client.ConnectAsync(token);
            _logger.Debug($"Connected to {cfg.Server}");

            await token;

            if (client.IsConnected)
            {
                _logger.Debug("Disconnecting");
                await client.DisconnectAsync(CancellationToken.None);
            }

            _logger.Debug("Disconnected");
        }
    }
}