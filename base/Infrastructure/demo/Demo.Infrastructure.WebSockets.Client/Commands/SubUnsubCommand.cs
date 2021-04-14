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
using NodaTime;

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

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken token)
        {
            var configuration = new ClientConfiguration().ConnectTo(cfg.Server).WithAutoReconnect().WithTimeout(Duration.FromSeconds(5));
            var client = _clientFactory.Create(configuration);
            client.ConnectionLost += () => _logger.Debug("connection lost");
            client.ConnectionRestored += () => _logger.Debug("connection restored");

            await client.ConnectAsync(token);

            _logger.Debug("Init subscription");
            var result = await client.SubscribeAsync(new UserBalanceSubscriptionInit(), (Action<UserBalanceMessage>) Log, token);
            _logger.Debug("Subscription initiated");

            await Task.Delay(3000);

            _logger.Debug("Cancel subscription");
            await client.UnsubscribeAsync(SubscriptionCancelRequest.New(result.Data), token);
            _logger.Debug("Subscription canceled");

            // _logger.Debug("Cancel subscription");
            // await client.UnsubscribeAsync(SubscriptionCancelRequest.New(Guid.NewGuid()), token);
            // _logger.Debug("Subscription canceled");

            await client.DisconnectAsync(token);

            void Log(UserBalanceMessage msg) => _logger.Debug($"<<< {msg}");
        }
    }
}