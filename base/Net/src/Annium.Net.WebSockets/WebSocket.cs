using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket, ILogSubject<WebSocket>
{
    public new ILogger<WebSocket> Logger { get; }

    public event Func<Task> ConnectionLost = () => Task.CompletedTask;

    public WebSocket(
        NativeWebSocket socket,
        ILoggerFactory loggerFactory
    ) : this(
        socket,
        new WebSocketOptions(),
        loggerFactory
    )
    {
    }

    public WebSocket(
        NativeWebSocket socket,
        WebSocketOptions options,
        ILoggerFactory loggerFactory
    ) : base(
        socket,
        options,
        Extensions.Execution.Executor.Background.Parallel<WebSocket>(),
        loggerFactory
    )
    {
        // resume observable unconditionally, because this kind of socket is expected to be connected
        if (Socket.State is not WebSocketState.Open)
            throw new WebSocketException("Unmanaged Socket must be already connected");

        Logger = loggerFactory.Get<WebSocket>();

        ResumeObservable();
    }

    public async Task DisconnectAsync()
    {
        this.Log().Trace("cancel receive, if pending, in {state}", Socket.State);
        PauseObservable();

        this.Log().Trace("invoke ConnectionLost in {state}", Socket.State);
        Executor.Schedule(() => ConnectionLost.Invoke());

        try
        {
            if (
                Socket.State == WebSocketState.Connecting ||
                Socket.State == WebSocketState.Open
            )
            {
                this.Log().Trace("Disconnect");
                await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
            }
            else
                this.Log().Trace("Already disconnected");
        }
        catch (WebSocketException)
        {
            this.Log().Trace(nameof(WebSocketException));
        }
    }

    protected override Task OnConnectionLostAsync()
    {
        this.Log().Trace("Invoke ConnectionLost");
        Executor.Schedule(() => ConnectionLost.Invoke());

        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        this.Log().Trace("start in {state}", Socket.State);
        if (Socket.State is WebSocketState.Connecting or WebSocketState.Open)
        {
            this.Log().Trace("Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());
        }

        await DisposeBaseAsync();

        this.Log().Trace("done");
    }
}