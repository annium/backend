using System.Net.WebSockets;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal sealed class ServerConnectionFactory : IServerConnectionFactory
{
    private readonly ServerTransportConfiguration _config;
    private readonly ILogger _logger;

    public ServerConnectionFactory(
        ServerTransportConfiguration config,
        ILogger logger
    )
    {
        _config = config;
        _logger = logger;
    }

    public IServerConnection Create(WebSocket socket)
    {
        var serverSocketOptions = new ServerWebSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
        };

        var serverSocket = new ServerWebSocket(socket, serverSocketOptions, _logger);

        return new ServerConnection(serverSocket, _logger);
    }
}