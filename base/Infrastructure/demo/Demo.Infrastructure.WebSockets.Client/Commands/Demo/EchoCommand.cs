using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo
{
    internal class EchoCommand : AsyncCommand<EchoCommandConfiguration>
    {
        private readonly ILogger<EchoCommand> _logger;
        public override string Id { get; } = "echo";
        public override string Description { get; } = "test echo flow";

        public EchoCommand(
            ILogger<EchoCommand> logger
        )
        {
            _logger = logger;
        }

        public override async Task HandleAsync(EchoCommandConfiguration cfg, CancellationToken token)
        {
            var ws = new ClientWebSocket(new ClientWebSocketOptions { ReconnectTimeout = TimeSpan.FromSeconds(1) });
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
            await ws.ConnectAsync(cfg.Server, token);
            _logger.Debug($"Connected to {cfg.Server}");

            _logger.Debug("Start echo loop");

            var sw = new Stopwatch();
            sw.Start();

            var value = 1;
            if (cfg.Delay > 0)
                while (!token.IsCancellationRequested)
                {
                    await Send(value++);
                    await Task.Delay(cfg.Delay, token);
                }
            else
                while (!token.IsCancellationRequested)
                {
                    await Send(value++);
                }

            sw.Stop();
            _logger.Debug($"Messages sent: {value}. Rate: {Math.Floor((double) value / sw.ElapsedMilliseconds * 1000)}rps");

            if (!cfg.Kill)
            {
                _logger.Debug("Disconnecting");
                await ws.DisconnectAsync(CancellationToken.None);
                _logger.Debug("Disconnected");
            }

            IObservable<Unit> Send(int val)
            {
                if (val % 10000 == 0)
                    _logger.Debug($">>> {val}");
                return ws.Send(val.ToString(), CancellationToken.None);
            }
        }
    }

    internal record EchoCommandConfiguration
    {
        [Option("s", isRequired: true)]
        [Help("Server address.")]
        public Uri Server { get; set; } = default!;

        [Option("d", isRequired: false)]
        [Help("Delay between messages.")]
        public int Delay { get; set; }

        [Option("k", isRequired: false)]
        [Help("Kill socket on end.")]
        public bool Kill { get; set; }
    }
}