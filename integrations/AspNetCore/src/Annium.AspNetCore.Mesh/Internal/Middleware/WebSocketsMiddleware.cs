using System;
using System.Net;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.Abstractions;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.Mesh.Internal.Middleware;

internal class WebSocketsMiddleware : IMiddleware, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServerConnectionFactory<WebSocket> _connectionFactory;
    private readonly ICoordinator _coordinator;
    private readonly WebSocketsMiddlewareConfiguration _config;
    private readonly Helper _helper;

    public WebSocketsMiddleware(
        IServerConnectionFactory<WebSocket> connectionFactory,
        ICoordinator coordinator,
        WebSocketsMiddlewareConfiguration config,
        IHostApplicationLifetime applicationLifetime,
        IIndex<SerializerKey, ISerializer<string>> serializers,
        ILogger logger
    )
    {
        Logger = logger;
        _connectionFactory = connectionFactory;
        _coordinator = coordinator;
        _config = config;
        applicationLifetime.ApplicationStopping.Register(_coordinator.Dispose);
        _helper = new Helper(
            serializers[SerializerKey.CreateDefault(MediaTypeNames.Application.Json)],
            MediaTypeNames.Application.Json
        );
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments(_config.PathMatch))
        {
            await next(context);
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
            this.Trace("accept");
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            this.Trace("create connection");
            var connection = await _connectionFactory.CreateAsync(webSocket);

            this.Trace("handle");
            await _coordinator.HandleAsync(connection);
        }
        catch (Exception ex)
        {
            this.Error(ex);
            await _helper.WriteResponse(
                context,
                HttpStatusCode.InternalServerError,
                Result.New().Error("WebSocket unhandled failure")
            );
        }
    }
}
