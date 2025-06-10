using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Servers.Web;

namespace Annium.Mesh.Server.Web.Internal;

/// <summary>
/// Handles incoming WebSocket connections for the mesh server by creating connection wrappers and delegating to the coordinator.
/// </summary>
public class Handler : IWebSocketHandler, ILogSubject
{
    /// <summary>
    /// Gets the logger for this handler.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The factory for creating server connections from WebSockets.
    /// </summary>
    private readonly IServerConnectionFactory<WebSocket> _connectionFactory;

    /// <summary>
    /// The coordinator for managing connection lifecycle.
    /// </summary>
    private readonly ICoordinator _coordinator;

    /// <summary>
    /// Initializes a new instance of the <see cref="Handler"/> class.
    /// </summary>
    /// <param name="sp">The service provider for resolving dependencies.</param>
    public Handler(IServiceProvider sp)
    {
        Logger = sp.Resolve<ILogger>();
        _connectionFactory = sp.Resolve<IServerConnectionFactory<WebSocket>>();
        _coordinator = sp.Resolve<ICoordinator>();
    }

    /// <summary>
    /// Handles an incoming WebSocket connection by wrapping it in a server connection and delegating to the coordinator.
    /// </summary>
    /// <param name="ctx">The WebSocket context containing the connection.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        try
        {
            this.Trace("start");

            this.Trace("create connection");
            var connection = await _connectionFactory.CreateAsync(ctx.WebSocket);

            this.Trace("handle connection");
            await _coordinator.HandleAsync(connection);

            this.Trace("done");
        }
        catch (Exception ex)
        {
            this.Error(ex);
        }
    }
}
