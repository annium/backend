using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo;

internal class EchoCommand : AsyncCommand<EchoCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "echo";
    public static string Description => "test echo flow";
    public ILogger Logger { get; }

    public EchoCommand(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(EchoCommandConfiguration cfg, CancellationToken ct)
    {
        var ws = new ClientWebSocket(
            new ClientWebSocketOptions { ReconnectTimeout = Duration.FromSeconds(1) },
            Logger
        );
        ws.ConnectionLost += () =>
        {
            this.Debug("connection lost");
            return Task.CompletedTask;
        };
        ws.ConnectionRestored += () =>
        {
            this.Debug("connection restored");
            return Task.CompletedTask;
        };

        this.Debug($"Connecting to {cfg.Server}");
        await ws.ConnectAsync(cfg.Server, ct);
        this.Debug($"Connected to {cfg.Server}");

        this.Debug("Start echo loop");

        var sw = new Stopwatch();
        sw.Start();

        var value = 1;
        if (cfg.Delay > 0)
            while (!ct.IsCancellationRequested)
            {
                await Send(value++);
                await Task.Delay(cfg.Delay, ct);
            }
        else
            while (!ct.IsCancellationRequested)
            {
                await Send(value++);
            }

        sw.Stop();
        this.Debug($"Messages sent: {value}. Rate: {Math.Floor((double)value / sw.ElapsedMilliseconds * 1000)}rps");

        if (!cfg.Kill)
        {
            this.Debug("Disconnecting");
            await ws.DisconnectAsync();
            this.Debug("Disconnected");
        }

        IObservable<Unit> Send(int val)
        {
            if (val % 10000 == 0)
                this.Debug($">>> {val}");
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