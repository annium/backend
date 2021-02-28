using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;
using Demo.Infrastructure.WebSockets.Domain.Responses.User;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class SubUnsubCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IIndex<string, ISerializer<ReadOnlyMemory<byte>>> _serializers;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "sub-unsub";
        public override string Description => $"test {Id} flow";

        public SubUnsubCommand(
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
            var configuration = new ClientConfiguration(cfg.Server, false, true, Duration.FromMinutes(5));
            var client = new ClientBase(configuration, _timeProvider, _serializers);
            client.ConnectionLost += () => _logger.Debug("connection lost");
            client.ConnectionRestored += () => _logger.Debug("connection restored");

            await client.ConnectAsync(token);

            _logger.Debug("Init subscription");
            var result = await client.Subscribe(new UserBalanceSubscriptionInit(), (Action<UserBalanceMessage>) Log, token);
            _logger.Debug("Subscription initiated");

            await Task.Delay(3000);

            _logger.Debug("Cancel subscription");
            await client.Unsubscribe(SubscriptionCancelRequest.New(result.Data), token);
            _logger.Debug("Subscription canceled");

            // _logger.Debug("Cancel subscription");
            // await client.Unsubscribe(SubscriptionCancelRequest.New(Guid.NewGuid()), token);
            // _logger.Debug("Subscription canceled");

            await client.DisconnectAsync(token);

            void Log(UserBalanceMessage msg) => _logger.Debug($"<<< {msg}");
        }
    }
}