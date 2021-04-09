using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Server;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.WebSockets.Internal.Middleware
{
    internal class WebSocketsMiddleware
    {
        private readonly ICoordinator _coordinator;
        private readonly ILogger<WebSocketsMiddleware> _logger;
        private readonly Helper _helper;

        public WebSocketsMiddleware(
            RequestDelegate next,
            ICoordinator coordinator,
            IHostApplicationLifetime applicationLifetime,
            IIndex<SerializerKey, ISerializer<string>> serializers,
            ILogger<WebSocketsMiddleware> logger
        )
        {
            _coordinator = coordinator;
            applicationLifetime.ApplicationStopping.Register(_coordinator.Shutdown);
            _logger = logger;
            _helper = new Helper(serializers[SerializerKey.CreateDefault(MediaTypeNames.Application.Json)], MediaTypeNames.Application.Json);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _helper.WriteResponse(
                    context,
                    HttpStatusCode.BadRequest,
                    Result.New().Error("Not a WebSocket connection")
                );
                return;
            }

            try
            {
                var socket = new WebSocket(await context.WebSockets.AcceptWebSocketAsync());
                await _coordinator.HandleAsync(socket);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                await _helper.WriteResponse(
                    context,
                    HttpStatusCode.InternalServerError,
                    Result.New().Error("WebSocket unhandled failure")
                );
            }
        }
    }
}