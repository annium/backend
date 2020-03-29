using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Annium.Net.WebSockets.DemoServer
{
    public class WebSocketEchoMiddleware
    {
        private readonly RequestDelegate next;

        public WebSocketEchoMiddleware(
            RequestDelegate next
        )
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            if (!path.StartsWithSegments("/ws"))
            {
                await next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return;
            }

            var ws = new WebSocket(await context.WebSockets.AcceptWebSocketAsync(), Serializers.Json);

            if (path == "/ws/echo")
                await Echo(ws);

            if (path == "/ws/data")
                await Data(ws);
        }

        private Task Echo(IWebSocket ws)
        {
            var tcs = new TaskCompletionSource<object>();

            ws.ListenText().Subscribe(
                async x =>
                {
                    Console.WriteLine($"In:  '{x}'");
                    await ws.Send(x, CancellationToken.None);
                    Console.WriteLine($"Out: '{x}'");
                },
                async () =>
                {
                    Console.WriteLine("Close");
                    await ws.DisconnectAsync(CancellationToken.None);
                    Console.WriteLine("Closed");
                    tcs.SetResult(new object());
                }
            );

            return tcs.Task;
        }

        private async Task Data(IWebSocket ws)
        {
            var isClosed = false;
            ws.ListenText().Subscribe(x => { }, () => isClosed = true);

            for (var i = 0; i < 1000; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (isClosed)
                {
                    Console.WriteLine("Closed");
                    break;
                }

                Console.WriteLine($"Out: '{i}'");
                await ws.Send(i, CancellationToken.None);
            }
        }
    }
}