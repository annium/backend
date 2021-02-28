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
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class RequestVoidCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IIndex<string, ISerializer<ReadOnlyMemory<byte>>> _serializers;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "request-void";
        public override string Description => $"test {Id} flow";

        public RequestVoidCommand(
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

            await client.ConnectAsync(token);

            var request = new DeleteOrderRequest();
            _logger.Debug($">>> {request}");
            var result = await client.Send(request);
            _logger.Debug($"<<< {result}");

            if (client.IsConnected)
                await client.DisconnectAsync(CancellationToken.None);
        }
    }
}