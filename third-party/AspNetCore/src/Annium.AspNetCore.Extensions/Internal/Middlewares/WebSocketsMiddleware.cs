using System;
using System.Net;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Extensions.Internal.Middlewares
{
    public class WebSocketsMiddleware
    {
        private readonly Helper _helper;

        public WebSocketsMiddleware(
            RequestDelegate next,
            IIndex<string, ISerializer<string>> serializers
        )
        {
            _helper = new Helper(serializers[MediaTypeNames.Application.Json], MediaTypeNames.Application.Json);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _helper.WriteResponse(context, HttpStatusCode.BadRequest, Result.New().Error("Not a WebSocket connection"));
                return;
            }

            using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.CloseStatus.HasValue) break;
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            } while (!result.CloseStatus.HasValue);

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}