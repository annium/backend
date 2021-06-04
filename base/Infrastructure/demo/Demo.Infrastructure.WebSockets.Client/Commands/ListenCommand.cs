using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Responses.System;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class ListenCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly IClientFactory _clientFactory;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "listen";
        public override string Description => $"test {Id} flow";

        public ListenCommand(
            IClientFactory clientFactory,
            ILogger<RequestCommand> logger
        )
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken ct)
        {
            var configuration = new ClientConfiguration()
                .ConnectTo(cfg.Server)
                .WithActiveKeepAlive(600)
                .WithResponseTimeout(600);
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
            await client.ConnectAsync(ct);
            _logger.Debug($"Connected to {cfg.Server}");

            using var _ = client.Listen<DiagnosticsNotification>().Subscribe(x => _logger.Debug($"<<< diagnostics: {x}"));

            await ct;
            _logger.Debug("Disconnecting");
            if (client.IsConnected)
                await client.DisconnectAsync();
            _logger.Debug("Disconnected");
        }
    }
}