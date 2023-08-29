using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;
using Demo.Infrastructure.WebSockets.Domain.Responses.User;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class SubUnsubCommand : AsyncCommand<ServerCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "sub-unsub";
    public static string Description => $"test {Id} flow";
    public ILogger Logger { get; }
    private readonly IClientFactory _clientFactory;

    public SubUnsubCommand(
        IClientFactory clientFactory,
        ILogger logger
    )
    {
        _clientFactory = clientFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken ct)
    {
        var configuration = new ClientConfiguration()
            .ConnectTo(cfg.Server)
            .WithActiveKeepAlive(600, 600)
            .WithResponseTimeout(600);
        await using var client = _clientFactory.Create(configuration);
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

        await client.ConnectAsync(ct);

        this.Debug("Init subscription");
        var result = await client.SubscribeAsync<UserBalanceSubscriptionInit, UserBalanceMessage>(new UserBalanceSubscriptionInit(), ct);
        if (result.IsOk)
        {
            var subscription = result.Data.Subscribe(Log);
            this.Debug("Subscription initiated");

            await Task.Delay(3000);

            this.Debug("Cancel subscription");
            subscription.Dispose();
            this.Debug("Subscription canceled");

            await Task.Delay(100);
        }
        else
            this.Error<string>("Subscription failed: {error}", result.PlainError);

        await client.DisconnectAsync();

        void Log(UserBalanceMessage msg) => this.Debug("<<< {msg}", msg);
    }
}