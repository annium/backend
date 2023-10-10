using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Mesh.Client;
using Demo.Mesh.Domain.Requests.Orders;

namespace Demo.Mesh.Client.Commands;

internal class RequestVoidCommand : AsyncCommand, ICommandDescriptor, ILogSubject
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        var configuration = new ClientConfiguration()
            .WithResponseTimeout(600);
        var client = _clientFactory.Create(configuration);

        await client.ConnectAsync();

        var request = new DeleteOrderRequest();
        this.Debug(">>> {request}", request);
        var result = await client.SendAsync(request, ct);
        this.Debug("<<< {result}", result);

        client.Disconnect();
    }
}