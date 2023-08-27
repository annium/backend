using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Net.WebSockets.Obsolete.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Obsolete;

[Obsolete]
public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
{
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;

    public WebSocket(
        NativeWebSocket socket
    ) : this(
        socket,
        new WebSocketOptions()
    )
    {
    }

    public WebSocket(
        NativeWebSocket socket,
        WebSocketOptions options
    ) : base(
        socket,
        Extensions.Execution.Executor.Background.Parallel<WebSocket>(),
        options,
        new WebSocketConfig
        {
            ResumeImmediately = true
        }
    )
    {
        // resume observable unconditionally, because this kind of socket is expected to be connected
        if (Socket.State is not WebSocketState.Open)
            throw new WebSocketException("Unmanaged Socket must be already connected");
    }

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
                this.TraceOld("Disconnect");
                await Socket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
            }
            else
                this.TraceOld("Already disconnected");
        }
        catch (WebSocketException)
        {
            this.TraceOld(nameof(WebSocketException));
        }
    }

    protected override Task OnConnectionLostAsync()
    {
        this.TraceOld("Invoke ConnectionLost");
        Executor.Schedule(() => ConnectionLost.Invoke());

        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        this.TraceOld($"start in {Socket.State}");
        if (Socket.State is WebSocketState.Connecting or WebSocketState.Open)
        {
            this.TraceOld("Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());
        }

        await DisposeBaseAsync();

        this.TraceOld("done");
    }
}