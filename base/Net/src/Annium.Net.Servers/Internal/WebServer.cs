using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Net.Servers.Internal;

internal class WebServer : IWebServer
{
    private readonly HttpListener _listener;
    private readonly Func<HttpListenerContext, ILogger, CancellationToken, Task> _handleHttpRequest;
    private readonly Func<HttpListenerContext, ILogger, CancellationToken, Task> _handleWebSocketRequest;
    private readonly Func<HttpListenerWebSocketContext, ILogger, CancellationToken, Task> _handleWebSocket;
    private readonly IBackgroundExecutor _executor;
    private readonly ILogger _logger;

    public WebServer(
        Uri uri,
        Func<HttpListenerContext, ILogger, CancellationToken, Task>? handleHttp,
        Func<HttpListenerWebSocketContext, ILogger, CancellationToken, Task>? handleWebSocket,
        ILogger logger
    )
    {
        _logger = logger;
        _listener = new HttpListener();
        _listener.Prefixes.Add(uri.ToString());
        _handleHttpRequest = handleHttp ?? CloseConnection;
        if (handleWebSocket is null)
        {
            _handleWebSocketRequest = CloseConnection;
            _handleWebSocket = IgnoreWebSocket;
        }
        else
        {
            _handleWebSocketRequest = HandleWebSocketRequest;
            _handleWebSocket = handleWebSocket;
        }

        _executor = Executor.Background.Parallel<WebServer>(logger);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        if (_listener.IsListening)
            throw new InvalidOperationException("Server is already started");

        _executor.Start(ct);
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext listenerContext;
            try
            {
                // await for connection
                listenerContext = await _listener.GetContextAsync().WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // schedule connection handling
            _executor.Schedule(async () => await HandleRequest(listenerContext, _logger, ct).ConfigureAwait(false));
        }

        // when cancelled - await connections processing and stop listener
        await _executor.DisposeAsync().ConfigureAwait(false);
        _listener.Stop();
    }

    private async Task HandleRequest(HttpListenerContext listenerContext, ILogger logger, CancellationToken ct)
    {
        if (listenerContext.Request.IsWebSocketRequest)
            await _handleWebSocketRequest(listenerContext, logger, ct);
        else
            await _handleHttpRequest(listenerContext, logger, ct);
    }

    private async Task HandleWebSocketRequest(HttpListenerContext listenerContext, ILogger logger, CancellationToken ct)
    {
        var statusCode = 200;
        var isAborted = false;
        try
        {
            var webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            await _handleWebSocket(webSocketContext, _logger, ct).ConfigureAwait(false);
            isAborted = webSocketContext.WebSocket.State is WebSocketState.Aborted;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            statusCode = 500;
        }
        finally
        {
            if (isAborted)
                listenerContext.Response.Abort();
            else
            {
                listenerContext.Response.StatusCode = statusCode;
                listenerContext.Response.Close();
            }
        }
    }

    private static Task CloseConnection(HttpListenerContext ctx, ILogger logger, CancellationToken ct)
    {
        ctx.Response.StatusCode = 404;
        ctx.Response.Close();

        return Task.CompletedTask;
    }

    private static Task IgnoreWebSocket(HttpListenerWebSocketContext ctx, ILogger logger, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}