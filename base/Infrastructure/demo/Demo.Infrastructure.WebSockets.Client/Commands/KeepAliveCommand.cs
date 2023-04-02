using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class KeepAliveCommand : AsyncCommand<ServerCommandConfiguration>, ICommandDescriptor, ILogSubject<KeepAliveCommand>
{
    public static string Id => "keep-alive";
    public static string Description => $"test {Id} flow";
    public ILogger<KeepAliveCommand> Logger { get; }
    private readonly IClientFactory _clientFactory;

    public KeepAliveCommand(
        IClientFactory clientFactory,
        ILogger<KeepAliveCommand> logger
    )
    {
        _clientFactory = clientFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken ct)
    {
        this.Log().Debug($"Interacting to {cfg.Server}");

        this.Log().Debug("create client");
        var configuration = new ClientConfiguration().ConnectTo(cfg.Server).WithActiveKeepAlive(1);
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

        while (true)
        {
            this.Log().Debug("press key");
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.C)
            {
                this.Log().Debug("connecting");
                await client.ConnectAsync(ct);
                this.Log().Debug("connected");
            }

            if (key == ConsoleKey.D)
            {
                this.Log().Debug("disconnecting");
                await client.DisconnectAsync();
                this.Log().Debug("disconnected");
            }

            if (key == ConsoleKey.Escape)
            {
                this.Log().Debug("break");
                break;
            }

            // await to avoid key pressing mess
            await Task.Delay(100, CancellationToken.None);
        }

        if (client.IsConnected)
        {
            this.Log().Debug("Disconnecting");
            await client.DisconnectAsync();
        }

        this.Log().Debug("Disconnected");
    }
}