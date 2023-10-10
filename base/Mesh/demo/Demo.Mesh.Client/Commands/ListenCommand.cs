using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Mesh.Client;
using Annium.Threading;
using Demo.Mesh.Domain.Responses.System;

namespace Demo.Mesh.Client.Commands;

internal class ListenCommand : AsyncCommand, ICommandDescriptor, ILogSubject
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        var configuration = new ClientConfiguration()
            .WithResponseTimeout(600);
        var client = _clientFactory.Create(configuration);
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

        this.Debug("Connecting to server");
        await client.ConnectAsync();
        this.Debug("Connected to server");

        using var _ = client
            .Listen<DiagnosticsNotification>()
            .Subscribe(x => this.Debug("<<< diagnostics: {x}", x));
        using var __ = client
            .Listen<SessionTimeNotification>()
            .Subscribe(x => this.Debug("<<< session: {x}", x));

        await ct;
        this.Debug("Disconnecting");
        client.Disconnect();
        this.Debug("Disconnected");
    }
}