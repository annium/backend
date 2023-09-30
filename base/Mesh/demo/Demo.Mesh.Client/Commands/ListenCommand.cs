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

        this.Debug("Connecting to {server}", cfg.Server);
        await client.ConnectAsync(ct);
        this.Debug("Connected to {server}", cfg.Server);

        using var _ = client
            .Listen<DiagnosticsNotification>()
            .Subscribe(x => this.Debug("<<< diagnostics: {x}", x));
        using var __ = client
            .Listen<SessionTimeNotification>()
            .Subscribe(x => this.Debug("<<< session: {x}", x));

        await ct;
        this.Debug("Disconnecting");
        await client.DisconnectAsync();
        this.Debug("Disconnected");
    }
}