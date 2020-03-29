using System;
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
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var ws = new WebSocket(await context.WebSockets.AcceptWebSocketAsync(), Serializers.Json);
                    await Echo(ws);
                }
                else
                {
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                }
            }
            else
            {
                await next(context);
            }
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
    }
}