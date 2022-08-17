using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

namespace Demo.Infrastructure.WebSockets.Client.Commands;

internal class RequestVoidCommand : AsyncCommand<ServerCommandConfiguration>, ILogSubject<RequestVoidCommand>
{
    public override string Id { get; } = "request-void";
    public override string Description => $"test {Id} flow";
    public ILogger<RequestVoidCommand> Logger { get; }
    private readonly IClientFactory _clientFactory;

    public RequestVoidCommand(
        IClientFactory clientFactory,
        ILogger<RequestVoidCommand> logger
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
        this.Log().Debug($">>> {request}");
        var result = await client.SendAsync(request, ct);
        this.Log().Debug($"<<< {result}");

        if (client.IsConnected)
            await client.DisconnectAsync();
    }
}