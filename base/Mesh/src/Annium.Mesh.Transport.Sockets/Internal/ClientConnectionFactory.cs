using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal sealed class ClientConnectionFactory : IClientConnectionFactory
{
    private readonly ClientTransportConfiguration _config;
    private readonly ILogger _logger;

    public ClientConnectionFactory(
        ClientTransportConfiguration config,
        ILogger logger
    )
    {
        _config = config;
        _logger = logger;
    }

    public IClientConnection Create()
    {
        var clientSocketOptions = new ClientSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
            ReconnectDelay = _config.ReconnectDelay
        };

        var clientSocket = new ClientSocket(clientSocketOptions, _logger);

        return new ClientConnection(clientSocket, _config.Endpoint, _config.AuthOptions, _logger);
    }
}