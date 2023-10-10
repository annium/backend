using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Mesh.Client;
using Demo.Mesh.Domain.Requests.User;
using Demo.Mesh.Domain.Responses.User;

namespace Demo.Mesh.Client.Commands;

internal class SubUnsubCommand : AsyncCommand, ICommandDescriptor, ILogSubject
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        var configuration = new ClientConfiguration()
            .WithResponseTimeout(600);
        await using var client = _clientFactory.Create(configuration);
        client.OnDisconnected += status =>
        {
            //
            this.Debug("disconnected: {status}", status);
        };
        client.OnConnected += () =>
        {
            //
            this.Debug("connected");
        };

        await client.ConnectAsync();

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

        client.Disconnect();

        void Log(UserBalanceMessage msg) => this.Debug("<<< {msg}", msg);
    }
}