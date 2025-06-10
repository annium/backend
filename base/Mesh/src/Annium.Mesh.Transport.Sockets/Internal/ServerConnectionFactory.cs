using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Factory for creating socket-based server connections.
/// </summary>
internal sealed class ServerConnectionFactory : IServerConnectionFactory<Socket>
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

    /// <summary>
    /// Creates a server connection asynchronously from an existing socket
    /// </summary>
    /// <param name="socket">The socket to create the connection from</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the server connection</returns>
    public async Task<IServerConnection> CreateAsync(Socket socket)
    {
        var serverSocketOptions = new ServerSocketOptions
        {
            Mode = SocketMode.Messaging,
            ConnectionMonitor = _config.ConnectionMonitor,
        };

        var stream = await GetStreamAsync(socket);
        var serverSocket = new ServerSocket(stream, serverSocketOptions, _logger);

        return new ServerConnection(serverSocket, _logger);
    }

    /// <summary>
    /// Creates a stream from the socket, optionally wrapping it with SSL/TLS.
    /// </summary>
    /// <param name="socket">The socket to create a stream from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a stream for communication, optionally SSL/TLS secured.</returns>
    private async Task<Stream> GetStreamAsync(Socket socket)
    {
        var networkStream = new NetworkStream(socket);

        if (_config.Certificate is null)
            return networkStream;

        var sslStream = new SslStream(networkStream, false);
        await sslStream.AuthenticateAsServerAsync(_config.Certificate);

        return sslStream;
    }
}
