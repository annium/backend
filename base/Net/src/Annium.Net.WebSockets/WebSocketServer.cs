using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public delegate void HandleClientConnected(WebSocket socket);

public class WebSocketServer
{
    public event HandleClientConnected OnConnected = delegate { };
    private readonly HttpListener _listener;

    public WebSocketServer(IPEndPoint endpoint, string path = "/")
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{endpoint}{path}");
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        if (_listener.IsListening)
            throw new InvalidOperationException("Server is already started");

        _listener.Start();
        ct.Register(_listener.Stop);

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext listenerContext;
            try
            {
                listenerContext = await _listener.GetContextAsync();
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            HandleRequest(listenerContext);
        }

        _listener.Stop();
    }


    private void HandleRequest(HttpListenerContext listenerContext) => Task.Run(async () =>
    {
        if (listenerContext.Request.IsWebSocketRequest)
        {
            try
            {
                var webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                OnConnected(webSocketContext.WebSocket);
            }
            catch (Exception)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
            }
        }
        else
        {
            listenerContext.Response.StatusCode = 400;
            listenerContext.Response.Close();
        }
    });
}