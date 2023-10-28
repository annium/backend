using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;
using ClientWebSocket = Annium.Net.WebSockets.ClientWebSocket;
using ClientWebSocketOptions = Annium.Net.WebSockets.ClientWebSocketOptions;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal sealed class ClientConnectionFactory : IClientConnectionFactory, IClientConnectionFactory<WebSocket>
{
    private readonly ClientTransportConfiguration _config;
    private readonly ILogger _logger;

    public ClientConnectionFactory(ClientTransportConfiguration config, ILogger logger)
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

    public Task<IManagedConnection> CreateAsync(WebSocket context)
    {
        var serverSocketOptions = new ServerWebSocketOptions { ConnectionMonitor = _config.ConnectionMonitor, };
        var serverSocket = new ServerWebSocket(context, serverSocketOptions, _logger);

        var connection = new ManagedConnection(serverSocket, _logger);

        return Task.FromResult<IManagedConnection>(connection);
    }
}
