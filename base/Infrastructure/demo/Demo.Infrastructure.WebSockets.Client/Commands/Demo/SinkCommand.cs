using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete;
using Annium.Serialization.Abstractions;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands.Demo;

internal class SinkCommand : AsyncCommand<SinkCommandConfiguration>, ICommandDescriptor, ILogSubject
{
    public static string Id => "sink";
    public static string Description => "socket sink (to listen broadcasts)";
    public ILogger Logger { get; }
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public SinkCommand(
        ISerializer<ReadOnlyMemory<byte>> serializer,
        ILogger logger
    )
    {
        _serializer = serializer;
        Logger = logger;
    }

    public override async Task HandleAsync(SinkCommandConfiguration cfg, CancellationToken ct)
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

        this.Debug("Connecting to {server}", cfg.Server);
        await ws.ConnectAsync(cfg.Server, ct);
        var count = 0;
        ws.ListenBinary().Select(_serializer.Deserialize<NotificationBase>).Subscribe(x =>
        {
            this.Debug("<<< {x}", x);
            count++;
        });
        this.Debug("Connected to {server}", cfg.Server);

        var sw = new Stopwatch();
        sw.Start();

        this.Debug("Demo start");
        try
        {
            await Task.Delay(-1, ct);
        }
        catch
        {
            // ignored
        }

        this.Debug("Demo end");

        sw.Stop();
        this.Debug("Messages received: {count}. Rate: {rate}rps", count, Math.Floor((double)count / sw.ElapsedMilliseconds * 1000));

        this.Debug("Disconnecting");
        await ws.DisconnectAsync();
        this.Debug("Disconnected");
    }
}

internal record SinkCommandConfiguration
{
    [Option("s", isRequired: true)]
    [Help("Server address.")]
    public Uri Server { get; set; } = default!;
}