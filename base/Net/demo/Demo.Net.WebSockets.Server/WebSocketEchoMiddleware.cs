using System;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Net.WebSockets;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Http;

namespace Demo.Net.WebSockets.Server
{
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

            var typeManager = TypeManager.GetInstance(typeof(Program).Assembly);
            var serializer = ByteArraySerializer.Configure(opts => opts.ConfigureDefault(typeManager));
            var ws = new WebSocket(await context.WebSockets.AcceptWebSocketAsync(), serializer);

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