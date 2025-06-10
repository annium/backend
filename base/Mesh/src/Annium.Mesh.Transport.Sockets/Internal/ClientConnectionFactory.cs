using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Factory for creating socket-based client connections.
/// </summary>
internal sealed class ClientConnectionFactory : IClientConnectionFactory, IClientConnectionFactory<Socket>
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
    /// Creates a new client connection with the configured settings
    /// </summary>
    /// <returns>A new client connection instance</returns>
    public IClientConnection Create()
    {
        var clientSocketOptions = new ClientSocketOptions
        {
            Mode = SocketMode.Messaging,
            ConnectionMonitor = _config.ConnectionMonitor,
            ReconnectDelay = _config.ReconnectDelay,
        };
        var clientSocket = new ClientSocket(clientSocketOptions, _logger);

        return new ClientConnection(clientSocket, _config.Uri, _config.AuthOptions, _logger);
    }

    /// <summary>
    /// Creates a managed connection asynchronously from an existing socket context
    /// </summary>
    /// <param name="context">The socket context to create the connection from</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the managed connection</returns>
    public async Task<IManagedConnection> CreateAsync(Socket context)
    {
        var serverSocketOptions = new ServerSocketOptions
        {
            Mode = SocketMode.Messaging,
            ConnectionMonitor = _config.ConnectionMonitor,
        };

        var stream = await GetStreamAsync(context);
        var serverSocket = new ServerSocket(stream, serverSocketOptions, _logger);

        return new ManagedConnection(serverSocket, _logger);
    }

    /// <summary>
    /// Creates a stream from the socket, optionally wrapping it with SSL/TLS.
    /// </summary>
    /// <param name="socket">The socket to create a stream from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a stream for communication, optionally SSL/TLS secured.</returns>
    private async Task<Stream> GetStreamAsync(Socket socket)
    {
        var networkStream = new NetworkStream(socket);

        if (_config.AuthOptions is null)
            return networkStream;

        var sslStream = new SslStream(
            networkStream,
            false,
            _config.AuthOptions.RemoteCertificateValidationCallback,
            null
        );
        await sslStream.AuthenticateAsClientAsync(_config.AuthOptions);

        return sslStream;
    }
}
