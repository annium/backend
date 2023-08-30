using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace Demo.Net.WebSockets.Server.Obsolete;

[Obsolete]
public class WebSocketEchoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public WebSocketEchoMiddleware(
        RequestDelegate next,
        ILogger logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/ws"))
        {
            await _next(context);
            return;
        }

        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var ws = new ServerWebSocket(await context.WebSockets.AcceptWebSocketAsync(), _logger);

        if (path == "/ws/echo")
            await Echo(ws);

        if (path == "/ws/data")
            await Data(ws);
    }

    private Task Echo(IServerWebSocket ws)
    {
        var tcs = new TaskCompletionSource<object>();

        ws.ObserveText()
            .DoSequentialAsync(async x =>
            {
                Console.WriteLine($"In:  '{x}'");
                await ws.SendTextAsync(x, CancellationToken.None);
                Console.WriteLine($"Out: '{x}'");
            })
            .Subscribe(_ =>
            {
                Console.WriteLine("Close");
                ws.Disconnect();
                Console.WriteLine("Closed");
                tcs.TrySetResult(new object());
            });

        return tcs.Task;
    }

    private async Task Data(IServerWebSocket ws)
    {
        var isClosed = false;
        ws.ObserveText().Subscribe(_ => { }, () => isClosed = true);

        for (var i = 0; i < 1000; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            if (isClosed)
            {
                Console.WriteLine("Closed");
                break;
            }

            Console.WriteLine($"Out: '{i}'");
            await ws.SendTextAsync(i.ToString(), CancellationToken.None);
        }
    }
}