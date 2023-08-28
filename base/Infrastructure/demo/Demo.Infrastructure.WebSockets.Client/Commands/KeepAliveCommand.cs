using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class KeepAliveCommand : AsyncCommand<ServerCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "keep-alive";
    public static string Description => $"test {Id} flow";
    public ILogger Logger { get; }
    private readonly IClientFactory _clientFactory;

    public KeepAliveCommand(
        IClientFactory clientFactory,
        ILogger logger
    )
    {
        _clientFactory = clientFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken ct)
    {
        this.Debug($"Interacting to {cfg.Server}");

        this.Debug("create client");
        var configuration = new ClientConfiguration().ConnectTo(cfg.Server).WithActiveKeepAlive(1);
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

        while (true)
        {
            this.Debug("press key");
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.C)
            {
                this.Debug("connecting");
                await client.ConnectAsync(ct);
                this.Debug("connected");
            }

            if (key == ConsoleKey.D)
            {
                this.Debug("disconnecting");
                await client.DisconnectAsync();
                this.Debug("disconnected");
            }

            if (key == ConsoleKey.Escape)
            {
                this.Debug("break");
                break;
            }

            // await to avoid key pressing mess
            await Task.Delay(100, CancellationToken.None);
        }

        if (client.IsConnected)
        {
            this.Debug("Disconnecting");
            await client.DisconnectAsync();
        }

        this.Debug("Disconnected");
    }
}