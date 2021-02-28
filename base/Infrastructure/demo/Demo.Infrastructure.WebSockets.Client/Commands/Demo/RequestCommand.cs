using System;
using System.Linq;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;
using Demo.Infrastructure.WebSockets.Domain.Requests.User;
using ClientWebSocket = Annium.Net.WebSockets.ClientWebSocket;
using ClientWebSocketOptions = Annium.Net.WebSockets.ClientWebSocketOptions;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo
{
    internal class RequestCommand : AsyncCommand<RequestCommandConfiguration>
    {
        private readonly ILogger<RequestCommand> _logger;
        private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;
        public override string Id { get; } = "request";
        public override string Description { get; } = "test demo flow";

        public RequestCommand(
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> serializers,
            ILogger<RequestCommand> logger
        )
        {
            _serializer = serializers[MediaTypeNames.Application.Json];
            _logger = logger;
        }

        public override async Task HandleAsync(RequestCommandConfiguration cfg, CancellationToken token)
        {
            var ws = new ClientWebSocket(new ClientWebSocketOptions
            {
                ReconnectOnFailure = true,
                OnConnectionLost = () =>
                {
                    _logger.Debug("connection lost");
                    return Task.CompletedTask;
                },
                OnConnectionRestored = () =>
                {
                    _logger.Debug("connection restored");
                    return Task.CompletedTask;
                },
            });

            _logger.Debug($"Connecting to {cfg.Server}");
            await ws.ConnectAsync(cfg.Server, token);
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
                        _logger.Error($"Deserialize failed with: {e}");
                        throw;
                    }
                })
                .Subscribe(x =>
                {
                    Interlocked.Increment(ref counter);
                    _logger.Debug($"<<< {x}");
                });
            _logger.Debug($"Connected to {cfg.Server}");

            _logger.Debug("Demo start");
            await Task.WhenAll(
                SendRequests(ws)
                // SendNotifications(ws),
            );
            _logger.Debug("Demo end");

            await Task.Delay(50);
            _logger.Debug($"Responses: {counter}");

            _logger.Debug("Disconnecting");
            if (ws.State == WebSocketState.Open)
                await ws.DisconnectAsync(CancellationToken.None);
            _logger.Debug("Disconnected");
        }

        private async Task Send<T>(ISendingWebSocket ws, T data)
            where T : notnull
        {
            var raw = _serializer.Serialize(data);
            _logger.Debug($">>> {data.GetType().FriendlyName()}::{data}");

            await ws.Send(raw, CancellationToken.None);
        }

        private async Task SendRequests(ISendingWebSocket ws)
        {
            await Task.WhenAll(Enumerable.Range(0, 10).Select(_ =>
                Send(ws, new DeleteOrderRequest {Id = Guid.NewGuid()})
            ));
        }

        private async Task SendNotifications(ISendingWebSocket ws)
        {
            for (var i = 0; i < 10; i++)
                await Send(ws, new UserActionNotification {Data = $"Action {i}"});
        }
    }

    internal record RequestCommandConfiguration
    {
        [Option("s", isRequired: true)]
        [Help("Server address.")]
        public Uri Server { get; set; } = default!;
    }
}