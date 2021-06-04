using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class RequestVoidCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly IClientFactory _clientFactory;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "request-void";
        public override string Description => $"test {Id} flow";

        public RequestVoidCommand(
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

            await client.ConnectAsync(ct);

            var request = new DeleteOrderRequest();
            _logger.Debug($">>> {request}");
            var result = await client.SendAsync(request, ct);
            _logger.Debug($"<<< {result}");

            if (client.IsConnected)
                await client.DisconnectAsync();
        }
    }
}