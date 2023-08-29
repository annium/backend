using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Obsolete;

[Obsolete]
public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
{
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;

    public WebSocket(
        NativeWebSocket socket,
        ILogger logger
    ) : this(
        socket,
        new WebSocketOptions(),
        logger
    )
    {
    }

    public WebSocket(
        NativeWebSocket socket,
        WebSocketOptions options,
        ILogger logger
    ) : base(
        socket,
        Extensions.Execution.Executor.Background.Parallel<WebSocket>(logger),
        options,
        new WebSocketConfig
        {
            ResumeImmediately = true
        },
        logger
    )
    {
        // resume observable unconditionally, because this kind of socket is expected to be connected
        if (Socket.State is not WebSocketState.Open)
            throw new WebSocketException("Unmanaged Socket must be already connected");
    }

    public async Task DisconnectAsync()
    {
        this.Trace("cancel receive, if pending, in {state}", Socket.State);
        PauseObservable();

        this.Trace("invoke ConnectionLost in {state}", Socket.State);
        Executor.Schedule(() => ConnectionLost.Invoke());

        try
        {
            if (
                Socket.State == WebSocketState.Connecting ||
                Socket.State == WebSocketState.Open
            )
            {
                this.Trace("Disconnect");
                await Socket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
            }
            else
                this.Trace("Already disconnected");
        }
        catch (WebSocketException)
        {
            this.Trace(nameof(WebSocketException));
        }
    }

    protected override Task OnConnectionLostAsync()
    {
        this.Trace("Invoke ConnectionLost");
        Executor.Schedule(() => ConnectionLost.Invoke());

        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        this.Trace("start in {state}", Socket.State);
        if (Socket.State is WebSocketState.Connecting or WebSocketState.Open)
        {
            this.Trace("Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());
        }

        await DisposeBaseAsync();

        this.Trace("done");
    }
}