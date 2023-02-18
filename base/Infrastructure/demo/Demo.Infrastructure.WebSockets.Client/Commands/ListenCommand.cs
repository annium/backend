using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Annium.Threading;
using Demo.Infrastructure.WebSockets.Domain.Responses.System;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class ListenCommand : AsyncCommand<ServerCommandConfiguration>, ILogSubject<ListenCommand>
{
    public override string Id { get; } = "listen";
    public override string Description => $"test {Id} flow";
    public ILogger<ListenCommand> Logger { get; }
    private readonly IClientFactory _clientFactory;

    public ListenCommand(
        IClientFactory clientFactory,
        ILogger<ListenCommand> logger
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
            this.Log().Debug("connection lost");
            return Task.CompletedTask;
        };
        client.ConnectionRestored += () =>
        {
            this.Log().Debug("connection restored");
            return Task.CompletedTask;
        };

        this.Log().Debug($"Connecting to {cfg.Server}");
        await client.ConnectAsync(ct);
        this.Log().Debug($"Connected to {cfg.Server}");

        using var _ = client.Listen<DiagnosticsNotification>().Subscribe(x => this.Log().Debug($"<<< diagnostics: {x}"));
        using var __ = client.Listen<SessionTimeNotification>().Subscribe(x => this.Log().Debug($"<<< session: {x}"));

        await ct;
        this.Log().Debug("Disconnecting");
        if (client.IsConnected)
            await client.DisconnectAsync();
        this.Log().Debug("Disconnected");
    }
}