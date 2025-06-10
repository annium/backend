using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;
using ClientWebSocket = Annium.Net.WebSockets.ClientWebSocket;
using ClientWebSocketOptions = Annium.Net.WebSockets.ClientWebSocketOptions;

namespace Annium.Mesh.Transport.WebSockets.Internal;

/// <summary>
/// Factory for creating WebSocket-based client connections.
/// </summary>
internal sealed class ClientConnectionFactory : IClientConnectionFactory, IClientConnectionFactory<WebSocket>
{
    /// <summary>
    /// The client transport configuration settings
    /// </summary>
    private readonly ClientTransportConfiguration _config;

    /// <summary>
    /// The logger instance for this factory
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnectionFactory"/> class.
    /// </summary>
    /// <param name="config">The client transport configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public ClientConnectionFactory(ClientTransportConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new WebSocket-based client connection with the configured settings
    /// </summary>
    /// <returns>A new client connection instance</returns>
    public IClientConnection Create()
    {
        var clientSocketOptions = new ClientWebSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
            ReconnectDelay = _config.ReconnectDelay,
        };
        var clientSocket = new ClientWebSocket(clientSocketOptions, _logger);

        return new ClientConnection(clientSocket, _config.Uri, _logger);
    }

    /// <inheritdoc />
    /// <summary>
    /// Creates a new WebSocket-based managed connection instance asynchronously.
    /// </summary>
    /// <param name="context">The WebSocket context to use for the connection.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the managed connection.</returns>
    public Task<IManagedConnection> CreateAsync(WebSocket context)
    {
        var serverSocketOptions = new ServerWebSocketOptions { ConnectionMonitor = _config.ConnectionMonitor };
        var serverSocket = new ServerWebSocket(context, serverSocketOptions, _logger);

        var connection = new ManagedConnection(serverSocket, _logger);

        return Task.FromResult<IManagedConnection>(connection);
    }
}
