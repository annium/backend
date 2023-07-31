using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Obsolete;
using Microsoft.AspNetCore.Http;

namespace Demo.Net.WebSockets.Server.Obsolete;

[Obsolete]
public class WebSocketEchoMiddleware
{
    private readonly RequestDelegate _next;

    public WebSocketEchoMiddleware(
        RequestDelegate next
    )
    {
        _next = next;
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
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            return;
        }

        var ws = new WebSocket(await context.WebSockets.AcceptWebSocketAsync());

        if (path == "/ws/echo")
            await Echo(ws);

        if (path == "/ws/data")
            await Data(ws);
    }

    private Task Echo(IWebSocket ws)
    {
        var tcs = new TaskCompletionSource<object>();

        ws.ListenText()
            .DoSequentialAsync(async x =>
            {
                Console.WriteLine($"In:  '{x}'");
                await ws.Send(x, CancellationToken.None);
                Console.WriteLine($"Out: '{x}'");
            })
            .SubscribeAsync(async () =>
            {
                Console.WriteLine("Close");
                await ws.DisconnectAsync();
                Console.WriteLine("Closed");
                tcs.TrySetResult(new object());
            });

        return tcs.Task;
    }

    private async Task Data(IWebSocket ws)
    {
        var isClosed = false;
        ws.ListenText().Subscribe(_ => { }, () => isClosed = true);

        for (var i = 0; i < 1000; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            if (isClosed)
            {
                Console.WriteLine("Closed");
                break;
            }

            Console.WriteLine($"Out: '{i}'");
            await ws.Send(i.ToString(), CancellationToken.None);
        }
    }
}