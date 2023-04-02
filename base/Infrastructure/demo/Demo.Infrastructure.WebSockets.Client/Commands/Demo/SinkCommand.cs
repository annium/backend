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

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo;

internal class SinkCommand : AsyncCommand<SinkCommandConfiguration>, ICommandDescriptor, ILogSubject<SinkCommand>
{
    public static string Id => "sink";
    public static string Description => "socket sink (to listen broadcasts)";
    public ILogger<SinkCommand> Logger { get; }
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public SinkCommand(
        ISerializer<ReadOnlyMemory<byte>> serializer,
        ILogger<SinkCommand> logger
    )
    {
        _serializer = serializer;
        Logger = logger;
    }

    public override async Task HandleAsync(SinkCommandConfiguration cfg, CancellationToken ct)
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
        var count = 0;
        ws.ListenBinary().Select(_serializer.Deserialize<NotificationBase>).Subscribe(x =>
        {
            this.Log().Debug($"<<< {x}");
            count++;
        });
        this.Log().Debug($"Connected to {cfg.Server}");

        var sw = new Stopwatch();
        sw.Start();

        this.Log().Debug("Demo start");
        try
        {
            await Task.Delay(-1, ct);
        }
        catch
        {
            // ignored
        }

        this.Log().Debug("Demo end");

        sw.Stop();
        this.Log().Debug($"Messages received: {count}. Rate: {Math.Floor((double) count / sw.ElapsedMilliseconds * 1000)}rps");

        this.Log().Debug("Disconnecting");
        await ws.DisconnectAsync();
        this.Log().Debug("Disconnected");
    }
}

internal record SinkCommandConfiguration
{
    [Option("s", isRequired: true)]
    [Help("Server address.")]
    public Uri Server { get; set; } = default!;
}