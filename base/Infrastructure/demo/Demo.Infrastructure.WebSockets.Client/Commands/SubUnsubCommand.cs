using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;
using Demo.Infrastructure.WebSockets.Domain.Responses.User;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class SubUnsubCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly IClientFactory _clientFactory;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "sub-unsub";
        public override string Description => $"test {Id} flow";

        public SubUnsubCommand(
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
                .WithActiveKeepAlive(5, 5)
                .WithResponseTimeout(5);
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

            await client.ConnectAsync(ct);

            _logger.Debug("Init subscription");
            var result = await client.SubscribeAsync(new UserBalanceSubscriptionInit(),
                (Action<UserBalanceMessage>) Log, ct);
            _logger.Debug("Subscription initiated");

            await Task.Delay(3000);

            _logger.Debug("Cancel subscription");
            await client.UnsubscribeAsync(SubscriptionCancelRequest.New(result.Data), ct);
            _logger.Debug("Subscription canceled");

            // _logger.Debug("Cancel subscription");
            // await client.UnsubscribeAsync(SubscriptionCancelRequest.New(Guid.NewGuid()), token);
            // _logger.Debug("Subscription canceled");

            await client.DisconnectAsync(ct);

            void Log(UserBalanceMessage msg) => _logger.Debug($"<<< {msg}");
        }
    }
}