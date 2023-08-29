using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class RequestVoidCommand : AsyncCommand<ServerCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "request-void";
    public static string Description => $"test {Id} flow";
    public ILogger Logger { get; }
    private readonly IClientFactory _clientFactory;

    public RequestVoidCommand(
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

        await client.ConnectAsync(ct);

        var request = new DeleteOrderRequest();
        this.Debug(">>> {request}", request);
        var result = await client.SendAsync(request, ct);
        this.Debug("<<< {result}", result);

        if (client.IsConnected)
            await client.DisconnectAsync();
    }
}