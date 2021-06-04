using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo
{
    internal class SinkCommand : AsyncCommand<SinkCommandConfiguration>
    {
        private readonly ILogger<SinkCommand> _logger;
        private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;
        public override string Id { get; } = "sink";
        public override string Description { get; } = "socket sink (to listen broadcasts)";

        public SinkCommand(
            ISerializer<ReadOnlyMemory<byte>> serializer,
            ILogger<SinkCommand> logger
        )
        {
            _serializer = serializer;
            _logger = logger;
        }

        public override async Task HandleAsync(SinkCommandConfiguration cfg, CancellationToken ct)
        {
            var ws = new ClientWebSocket(new ClientWebSocketOptions { ReconnectTimeout = Duration.FromSeconds(1) });
            ws.ConnectionLost += () =>
            {
                _logger.Debug("connection lost");
                return Task.CompletedTask;
            };
            ws.ConnectionRestored += () =>
            {
                _logger.Debug("connection restored");
                return Task.CompletedTask;
            };

            _logger.Debug($"Connecting to {cfg.Server}");
            await ws.ConnectAsync(cfg.Server, ct);
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
                await Task.Delay(-1, ct);
            }
            catch
            {
                // ignored
            }

            _logger.Debug("Demo end");

            sw.Stop();
            _logger.Debug($"Messages received: {count}. Rate: {Math.Floor((double) count / sw.ElapsedMilliseconds * 1000)}rps");

            _logger.Debug("Disconnecting");
            await ws.DisconnectAsync();
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