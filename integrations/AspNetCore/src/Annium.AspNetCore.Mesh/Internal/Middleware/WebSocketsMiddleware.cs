using System;
using System.Net;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.Abstractions;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.Mesh.Internal.Middleware;

/// <summary>
/// Middleware that handles WebSocket connections for the Mesh server
/// </summary>
internal class WebSocketsMiddleware : IMiddleware, ILogSubject
{
    /// <summary>
    /// Gets the logger for this middleware
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The factory for creating WebSocket server connections
    /// </summary>
    private readonly IServerConnectionFactory<WebSocket> _connectionFactory;

    /// <summary>
    /// The coordinator for handling connections
    /// </summary>
    private readonly ICoordinator _coordinator;

    /// <summary>
    /// The configuration for the WebSocket middleware
    /// </summary>
    private readonly WebSocketsMiddlewareConfiguration _config;

    /// <summary>
    /// The helper for writing HTTP responses
    /// </summary>
    private readonly Helper _helper;

    /// <summary>
    /// Initializes a new instance of the WebSocketsMiddleware class
    /// </summary>
    /// <param name="sp">The service provider for dependency resolution</param>
    /// <param name="connectionFactory">The factory for creating server connections</param>
    /// <param name="coordinator">The coordinator for handling connections</param>
    /// <param name="config">The middleware configuration</param>
    /// <param name="applicationLifetime">The application lifetime for cleanup</param>
    /// <param name="logger">The logger for error reporting</param>
    public WebSocketsMiddleware(
        IServiceProvider sp,
        IServerConnectionFactory<WebSocket> connectionFactory,
        ICoordinator coordinator,
        WebSocketsMiddlewareConfiguration config,
        IHostApplicationLifetime applicationLifetime,
        ILogger logger
    )
    {
        Logger = logger;
        _connectionFactory = connectionFactory;
        _coordinator = coordinator;
        _config = config;
        applicationLifetime.ApplicationStopping.Register(_coordinator.Dispose);
        var serializerKey = SerializerKey.CreateDefault(MediaTypeNames.Application.Json);
        var serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
        _helper = new Helper(serializer, MediaTypeNames.Application.Json);
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request and WebSocket upgrade
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments(_config.PathMatch))
        {
            await next(context);
            return;
        }

        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _helper.WriteResponseAsync(
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
            await _helper.WriteResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                Result.New().Error("WebSocket unhandled failure")
            );
        }
    }
}
