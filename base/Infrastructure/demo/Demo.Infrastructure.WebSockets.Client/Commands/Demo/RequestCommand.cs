using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;
using NodaTime;
using ClientWebSocket = Annium.Net.WebSockets.ClientWebSocket;
using ClientWebSocketOptions = Annium.Net.WebSockets.ClientWebSocketOptions;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo;

internal class RequestCommand : AsyncCommand<RequestCommandConfiguration>, ICommandDescriptor, ILogSubject<RequestCommand>
{
    public static string Id => "request";
    public static string Description => "test demo flow";
    public ILogger<RequestCommand> Logger { get; }
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public RequestCommand(
        ISerializer<ReadOnlyMemory<byte>> serializer,
        ILogger<RequestCommand> logger
    )
    {
        _serializer = serializer;
        Logger = logger;
    }

    public override async Task HandleAsync(RequestCommandConfiguration cfg, CancellationToken ct)
    {
        var ws = new ClientWebSocket(
            new ClientWebSocketOptions { ReconnectTimeout = Duration.FromSeconds(1) }
        );
        ws.ConnectionLost += () =>
        {
            this.Log().Debug("connection lost");
            return Task.CompletedTask;
        };
        ws.ConnectionRestored += () =>
        {
            this.Log().Debug("connection restored");
            return Task.CompletedTask;
        };

        this.Log().Debug($"Connecting to {cfg.Server}");
        await ws.ConnectAsync(cfg.Server, ct);
        var counter = 0;
        ws.ListenBinary()
            .Select(x =>
            {
                try
                {
                    return _serializer.Deserialize<AbstractResponseBase>(x);
                }
                catch (Exception e)
                {
                    this.Log().Error($"Deserialize failed with: {e}");
                    throw;
                }
            })
            .Subscribe(x =>
            {
                Interlocked.Increment(ref counter);
                this.Log().Debug($"<<< {x}");
            });
        this.Log().Debug($"Connected to {cfg.Server}");

        this.Log().Debug("Demo start");
        await Task.WhenAll(
            SendRequests(ws)
            // SendNotifications(ws),
        );
        this.Log().Debug("Demo end");

        await Task.Delay(50);
        this.Log().Debug($"Responses: {counter}");

        this.Log().Debug("Disconnecting");
        if (ws.State == WebSocketState.Open)
            await ws.DisconnectAsync();
        this.Log().Debug("Disconnected");
    }

    private async Task Send<T>(ISendingWebSocket ws, T data)
        where T : notnull
    {
        var raw = _serializer.Serialize(data);
        this.Log().Debug($">>> {data.GetType().FriendlyName()}::{data}");

        await ws.Send(raw, CancellationToken.None);
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