using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal sealed class ClientConnectionFactory : IClientConnectionFactory
{
    private readonly TransportConfiguration _config;
    private readonly ILogger _logger;

    public ClientConnectionFactory(
        TransportConfiguration config,
        ILogger logger
    )
    {
        _config = config;
        _logger = logger;
    }

    public IClientConnection Create()
    {
        var clientSocketOptions = new ClientWebSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
            ReconnectDelay = _config.ReconnectDelay
        };

        var clientSocket = new ClientWebSocket(clientSocketOptions, _logger);

        return new ClientConnection(clientSocket, _config.Uri, _logger);
    }
}