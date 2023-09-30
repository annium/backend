using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Mesh.Domain.Responses;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;

namespace Demo.Mesh.Client.Commands.Demo;

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
        var ws = new ClientWebSocket(ClientWebSocketOptions.Default, Logger);
        ws.OnDisconnected += status => this.Debug("connection lost: {status}", status);
        ws.OnConnected += () => this.Debug("connection restored");

        this.Debug("Connecting to {server}", cfg.Server);
        ws.Connect(cfg.Server);
        var count = 0;
        ws.ObserveBinary().Select(_serializer.Deserialize<NotificationBase>).Subscribe(x =>
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
        ws.Disconnect();
        this.Debug("Disconnected");
    }
}

internal record SinkCommandConfiguration
{
    [Option("s", isRequired: true)]
    [Help("Server address.")]
    public Uri Server { get; set; } = default!;
}