using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Net.WebSockets;

namespace Demo.Mesh.Client.Commands.Demo;

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
        var ws = new ClientWebSocket(ClientWebSocketOptions.Default, Logger);
        ws.OnDisconnected += status => this.Debug("connection lost: {status}", status);
        ws.OnConnected += () => this.Debug("connection restored");

        this.Debug("Connecting to {server}", cfg.Server);
        ws.Connect(cfg.Server);
        this.Debug("Connected to {server}", cfg.Server);

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
        this.Debug("Messages sent: {value}. Rate: {rate}rps", value, Math.Floor((double)value / sw.ElapsedMilliseconds * 1000));

        if (!cfg.Kill)
        {
            this.Debug("Disconnecting");
            ws.Disconnect();
            this.Debug("Disconnected");
        }

        return;

        ValueTask<WebSocketSendStatus> Send(int val)
        {
            if (val % 10000 == 0)
                this.Debug(">>> {val}", val);
            return ws.SendBinaryAsync(val.ToString(), CancellationToken.None);
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