using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;

namespace Annium.Net.WebSockets;

public class WebSocketServer
{
    private readonly Func<WebSocket, CancellationToken, Task> _handleClient;
    private readonly HttpListener _listener;
    private readonly IBackgroundExecutor _executor = Executor.Background.Parallel<WebSocketServer>();

    public WebSocketServer(IPEndPoint endpoint, string path, Func<WebSocket, CancellationToken, Task> handleClient)
    {
        _handleClient = handleClient;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{endpoint}{path}");
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
            _executor.Schedule(async () => await HandleRequest(listenerContext, ct).ConfigureAwait(false));
        }

        // when cancelled - await connections processing and stop listener
        await _executor.DisposeAsync().ConfigureAwait(false);
        _listener.Stop();
    }


    private async Task HandleRequest(HttpListenerContext listenerContext, CancellationToken ct)
    {
        var statusCode = 200;
        try
        {
            if (!listenerContext.Request.IsWebSocketRequest)
            {
                statusCode = 400;
                return;
            }

            var webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            await _handleClient(webSocketContext.WebSocket, ct).ConfigureAwait(false);
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
            listenerContext.Response.StatusCode = statusCode;
            listenerContext.Response.Close();
        }
    }
}