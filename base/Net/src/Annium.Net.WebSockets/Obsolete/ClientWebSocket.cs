using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Net.WebSockets.Obsolete.Internal;
using NodaTime;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Obsolete;

[Obsolete]
public class ClientWebSocket : WebSocketBase<NativeClientWebSocket>, IClientWebSocket
{
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;
    public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
    private readonly ClientWebSocketOptions _options;
    private Uri? _uri;

    public ClientWebSocket(
        ClientWebSocketOptions options
    ) : base(
        new NativeClientWebSocket(),
        Extensions.Execution.Executor.Background.Parallel<ClientWebSocket>(),
        options,
        new WebSocketConfig
        {
            ResumeImmediately = false
        }
    )
    {
        _options = options;
    }

    public ClientWebSocket(
    ) : this(new ClientWebSocketOptions())
    {
    }

    public Task ConnectAsync(Uri uri, CancellationToken ct) =>
        ConnectAsync(uri, _options.ConnectTimeout, ct);

    public async Task DisconnectAsync()
    {
        this.TraceOld($"cancel receive, if pending, in {Socket.State}");
        PauseObservable();

        this.TraceOld($"invoke ConnectionLost in {Socket.State}");
        Executor.Schedule(() => ConnectionLost.Invoke());

        try
        {
            if (
                Socket.State == WebSocketState.Connecting ||
                Socket.State == WebSocketState.Open
            )
            {
                this.TraceOld("disconnect");
                await Socket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
            }
            else
                this.TraceOld("already disconnected");
        }
        catch (WebSocketException)
        {
            this.TraceOld(nameof(WebSocketException));
        }
    }

    protected override async Task OnConnectionLostAsync()
    {
        this.TraceOld("invoke ConnectionLost");
        Executor.Schedule(() => ConnectionLost.Invoke());

        if (_options.ReconnectTimeout != Duration.MaxValue)
        {
            this.TraceOld("try reconnect");
            await Task.Delay(_options.ReconnectTimeout.ToTimeSpan());
            await ConnectAsync(_uri!, _options.ReconnectTimeout, CancellationToken.None);
        }
        else
            this.TraceOld("no reconnect");
    }

    private async Task ConnectAsync(Uri uri, Duration timeout, CancellationToken ct)
    {
        this.TraceOld($"connect to {uri}");

        _uri = uri;
        do
        {
            try
            {
                Socket = new NativeClientWebSocket();
                this.TraceOld("try connect");
                await Socket.ConnectAsync(uri, ct);
            }
            catch (WebSocketException)
            {
                this.TraceOld("connection failed");
                Socket.Dispose();
                await Task.Delay(timeout.ToTimeSpan(), ct);
            }
        }
        while (
            !ct.IsCancellationRequested &&
            Socket.State is not (WebSocketState.Open or WebSocketState.CloseSent)
        );

        if (Socket.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            this.TraceOld("connected");
            ResumeObservable();

            this.TraceOld("invoke ConnectionRestored");
            Executor.Schedule(() => ConnectionRestored.Invoke());
        }
        else
            this.TraceOld("connected");
    }

    public override async ValueTask DisposeAsync()
    {
        this.TraceOld($"start in {Socket.State}");
        if (Socket.State is WebSocketState.Connecting or WebSocketState.Open)
        {
            this.TraceOld("invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());
        }

        await DisposeBaseAsync();

        this.TraceOld("done");
    }
}