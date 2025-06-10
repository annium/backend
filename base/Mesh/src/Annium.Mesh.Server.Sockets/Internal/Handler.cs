using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Servers.Sockets;

namespace Annium.Mesh.Server.Sockets.Internal;

/// <summary>
/// Handles incoming socket connections for the mesh server by creating connection wrappers and delegating to the coordinator.
/// </summary>
internal class Handler : IHandler, ILogSubject
{
    /// <summary>
    /// Gets the logger for this handler.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The factory for creating server connections from sockets.
    /// </summary>
    private readonly IServerConnectionFactory<Socket> _connectionFactory;

    /// <summary>
    /// The coordinator for managing connection lifecycle.
    /// </summary>
    private readonly ICoordinator _coordinator;

    /// <summary>
    /// Initializes a new instance of the <see cref="Handler"/> class.
    /// </summary>
    /// <param name="connectionFactory">The factory for creating server connections from sockets.</param>
    /// <param name="coordinator">The coordinator for managing connection lifecycle.</param>
    /// <param name="logger">The logger for this handler.</param>
    public Handler(IServerConnectionFactory<Socket> connectionFactory, ICoordinator coordinator, ILogger logger)
    {
        Logger = logger;
        _connectionFactory = connectionFactory;
        _coordinator = coordinator;
    }

    /// <summary>
    /// Handles an incoming socket connection by wrapping it in a server connection and delegating to the coordinator.
    /// </summary>
    /// <param name="socket">The socket to handle.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(Socket socket, CancellationToken ct)
    {
        try
        {
            this.Trace("start");

            this.Trace("create connection");
            var connection = await _connectionFactory.CreateAsync(socket);

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
