using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo;

internal class RequestCommand : AsyncCommand<RequestCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "request";
    public static string Description => "test demo flow";
    public ILogger Logger { get; }
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public RequestCommand(
        ISerializer<ReadOnlyMemory<byte>> serializer,
        ILogger logger
    )
    {
        _serializer = serializer;
        Logger = logger;
    }

    public override async Task HandleAsync(RequestCommandConfiguration cfg, CancellationToken ct)
    {
        var ws = new ClientWebSocket(ClientWebSocketOptions.Default, Logger);
        ws.OnDisconnected += status => this.Debug("connection lost: {status}", status);
        ws.OnConnected += () => this.Debug("connection restored");

        this.Debug("Connecting to {server}", cfg.Server);
        ws.Connect(cfg.Server);
        var counter = 0;
        ws.ObserveBinary()
            .Select(x =>
            {
                try
                {
                    return _serializer.Deserialize<AbstractResponseBase>(x);
                }
                catch (Exception e)
                {
                    this.Error("Deserialize failed with: {e}", e);
                    throw;
                }
            })
            .Subscribe(x =>
            {
                Interlocked.Increment(ref counter);
                this.Debug("<<< {x}", x);
            });
        this.Debug("Connected to {server}", cfg.Server);

        this.Debug("Demo start");
        await Task.WhenAll(
            SendRequests(ws)
            // SendNotifications(ws),
        );
        this.Debug("Demo end");

        await Task.Delay(50);
        this.Debug("Responses: {counter}", counter);

        this.Debug("Disconnecting");
        ws.Disconnect();
        this.Debug("Disconnected");
    }

    private async Task Send<T>(ISendingWebSocket ws, T data)
        where T : notnull
    {
        var raw = _serializer.Serialize(data);
        this.Debug(">>> {data.GetType().FriendlyName()}::{data}", data.GetType().FriendlyName(), data);

        await ws.SendBinaryAsync(raw, CancellationToken.None);
    }

    private async Task SendRequests(ISendingWebSocket ws)
    {
        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ =>
            Send(ws, new DeleteOrderRequest { Id = Guid.NewGuid() })
        ));
    }

    private async Task SendNotifications(ISendingWebSocket ws)
    {
        for (var i = 0; i < 10; i++)
            await Send(ws, new UserActionNotification { Data = $"Action {i}" });
    }
}

internal record RequestCommandConfiguration
{
    [Option("s", isRequired: true)]
    [Help("Server address.")]
    public Uri Server { get; set; } = default!;
}