using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo
{
    internal class SinkCommand : AsyncCommand<SinkCommandConfiguration>
    {
        private readonly ILogger<SinkCommand> _logger;
        private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;
        public override string Id { get; } = "sink";
        public override string Description { get; } = "socket sink (to listen broadcasts)";

        public SinkCommand(
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> serializers,
            ILogger<SinkCommand> logger
        )
        {
            _serializer = serializers[MediaTypeNames.Application.Json];
            _logger = logger;
        }

        public override async Task HandleAsync(SinkCommandConfiguration cfg, CancellationToken token)
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
            var count = 0;
            ws.ListenBinary().Select(_serializer.Deserialize<NotificationBase>).Subscribe(x =>
            {
                _logger.Debug($"<<< {x}");
                count++;
            });
            _logger.Debug($"Connected to {cfg.Server}");

            var sw = new Stopwatch();
            sw.Start();

            _logger.Debug("Demo start");
            try
            {
                await Task.Delay(-1, token);
            }
            catch
            {
                // ignored
            }

            _logger.Debug("Demo end");

            sw.Stop();
            _logger.Debug($"Messages received: {count}. Rate: {Math.Floor((double) count / sw.ElapsedMilliseconds * 1000)}rps");

            _logger.Debug("Disconnecting");
            await ws.DisconnectAsync(CancellationToken.None);
            _logger.Debug("Disconnected");
        }
    }

    internal record SinkCommandConfiguration
    {
        [Option("s", isRequired: true)]
        [Help("Server address.")]
        public Uri Server { get; set; } = default!;
    }
}