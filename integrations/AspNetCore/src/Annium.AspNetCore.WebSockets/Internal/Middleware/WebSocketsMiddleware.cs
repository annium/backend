using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Server;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets.Obsolete;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.WebSockets.Internal.Middleware;

internal class WebSocketsMiddleware : ILogSubject<WebSocketsMiddleware>
{
    public ILogger<WebSocketsMiddleware> Logger { get; }
    private readonly RequestDelegate _next;
    private readonly ICoordinator _coordinator;
    private readonly ServerConfiguration _cfg;
    private readonly Helper _helper;

    public WebSocketsMiddleware(
        RequestDelegate next,
        ICoordinator coordinator,
        ServerConfiguration cfg,
        IHostApplicationLifetime applicationLifetime,
        IIndex<SerializerKey, ISerializer<string>> serializers,
        ILogger<WebSocketsMiddleware> logger
    )
    {
        _next = next;
        _coordinator = coordinator;
        _cfg = cfg;
        applicationLifetime.ApplicationStopping.Register(_coordinator.Shutdown);
        Logger = logger;
        _helper = new Helper(
            serializers[SerializerKey.CreateDefault(MediaTypeNames.Application.Json)],
            MediaTypeNames.Application.Json
        );
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(_cfg.PathMatch))
        {
            await _next(context);
            return;
        }

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
            this.Log().Trace("accept");
            var rawSocket = await context.WebSockets.AcceptWebSocketAsync();
            this.Log().Trace("create socket");
            var socket = new WebSocket(rawSocket, _cfg.WebSocketOptions);
            this.Log().Trace("handle");
            await _coordinator.HandleAsync(socket);
        }
        catch (Exception ex)
        {
            this.Log().Error(ex);
            await _helper.WriteResponse(
                context,
                HttpStatusCode.InternalServerError,
                Result.New().Error("WebSocket unhandled failure")
            );
        }
    }
}