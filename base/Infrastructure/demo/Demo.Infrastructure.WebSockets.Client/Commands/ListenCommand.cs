using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging;
using Annium.Threading;
using Demo.Infrastructure.WebSockets.Domain.Responses.System;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class ListenCommand : AsyncCommand<ServerCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "listen";
    public static string Description => $"test {Id} flow";
    public ILogger Logger { get; }
    private readonly IClientFactory _clientFactory;

    public ListenCommand(
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
            .WithActiveKeepAlive(600)
            .WithResponseTimeout(600);
        var client = _clientFactory.Create(configuration);
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

        this.Debug($"Connecting to {cfg.Server}");
        await client.ConnectAsync(ct);
        this.Debug($"Connected to {cfg.Server}");

        using var _ = client.Listen<DiagnosticsNotification>().Subscribe(x => this.Debug($"<<< diagnostics: {x}"));
        using var __ = client.Listen<SessionTimeNotification>().Subscribe(x => this.Debug($"<<< session: {x}"));

        await ct;
        this.Debug("Disconnecting");
        if (client.IsConnected)
            await client.DisconnectAsync();
        this.Debug("Disconnected");
    }
}