using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal sealed class ServerConnectionFactory : IServerConnectionFactory<WebSocket>
{
    private readonly ServerTransportConfiguration _config;
    private readonly ILogger _logger;

    public ServerConnectionFactory(ServerTransportConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task<IServerConnection> CreateAsync(WebSocket socket)
    {
        var serverSocketOptions = new ServerWebSocketOptions { ConnectionMonitor = _config.ConnectionMonitor, };

        var serverSocket = new ServerWebSocket(socket, serverSocketOptions, _logger);
        var connection = new ServerConnection(serverSocket, _logger);

        return Task.FromResult<IServerConnection>(connection);
    }
}
