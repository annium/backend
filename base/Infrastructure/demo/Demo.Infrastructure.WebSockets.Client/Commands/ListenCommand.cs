using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Responses.System;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class ListenCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IIndex<string, ISerializer<ReadOnlyMemory<byte>>> _serializers;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "listen";
        public override string Description => $"test {Id} flow";

        public ListenCommand(
            ITimeProvider timeProvider,
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> serializers,
            ILogger<RequestCommand> logger
        )
        {
            _timeProvider = timeProvider;
            _serializers = serializers;
            _logger = logger;
        }

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken token)
        {
            var configuration = new ClientConfiguration(cfg.Server, false, true, Duration.FromSeconds(5));
            var client = new ClientBase(configuration, _timeProvider, _serializers);
            client.ConnectionLost += () => _logger.Debug("connection lost");
            client.ConnectionRestored += () => _logger.Debug("connection restored");

            _logger.Debug($"Connecting to {cfg.Server}");
            await client.ConnectAsync(token);
            _logger.Debug($"Connected to {cfg.Server}");

            token.Register(client.Listen<DiagnosticsNotification>(
                x => _logger.Debug($"<<< diagnostics: {x}")
            ));

            token.WaitHandle.WaitOne();
            _logger.Debug("Disconnecting");
            if (client.IsConnected)
                await client.DisconnectAsync(CancellationToken.None);
            _logger.Debug("Disconnected");
        }
    }
}