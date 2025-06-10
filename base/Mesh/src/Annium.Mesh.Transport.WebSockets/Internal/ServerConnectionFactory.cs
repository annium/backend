using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

/// <summary>
/// Factory for creating WebSocket-based server connections.
/// </summary>
internal sealed class ServerConnectionFactory : IServerConnectionFactory<WebSocket>
{
    /// <summary>
    /// The server transport configuration settings
    /// </summary>
    private readonly ServerTransportConfiguration _config;

    /// <summary>
    /// The logger instance for this factory
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConnectionFactory"/> class.
    /// </summary>
    /// <param name="config">The server transport configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public ServerConnectionFactory(ServerTransportConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <summary>
    /// Creates a new WebSocket-based server connection instance asynchronously.
    /// </summary>
    /// <param name="socket">The WebSocket to use for the connection.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the server connection.</returns>
    public Task<IServerConnection> CreateAsync(WebSocket socket)
    {
        var serverSocketOptions = new ServerWebSocketOptions { ConnectionMonitor = _config.ConnectionMonitor };

        var serverSocket = new ServerWebSocket(socket, serverSocketOptions, _logger);
        var connection = new ServerConnection(serverSocket, _logger);

        return Task.FromResult<IServerConnection>(connection);
    }
}
