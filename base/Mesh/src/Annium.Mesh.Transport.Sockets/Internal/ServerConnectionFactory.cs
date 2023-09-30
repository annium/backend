using System.Net.Sockets;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal sealed class ServerConnectionFactory : IServerConnectionFactory
{
    private readonly TransportConfiguration _config;
    private readonly ILogger _logger;

    public ServerConnectionFactory(
        TransportConfiguration config,
        ILogger logger
    )
    {
        _config = config;
        _logger = logger;
    }

    public IServerConnection Create(Socket socket)
    {
        var serverSocketOptions = new ServerSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
        };

        var serverSocket = new ServerSocket(socket, serverSocketOptions, _logger);

        return new ServerConnection(serverSocket, _logger);
    }
}