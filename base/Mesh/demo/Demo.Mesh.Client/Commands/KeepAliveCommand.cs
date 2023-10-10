using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Mesh.Client;

namespace Demo.Mesh.Client.Commands;

internal class KeepAliveCommand : AsyncCommand, ICommandDescriptor, ILogSubject
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        this.Debug("Interacting to server");

        this.Debug("create client");
        var configuration = new ClientConfiguration();
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

        while (true)
        {
            this.Debug("press key");
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.C)
            {
                this.Debug("connecting");
                await client.ConnectAsync();
                this.Debug("connected");
            }

            if (key == ConsoleKey.D)
            {
                this.Debug("disconnecting");
                client.Disconnect();
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

        this.Debug("Disconnecting");
        client.Disconnect();

        this.Debug("Disconnected");
    }
}