using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

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

    public async Task<IServerConnection> CreateAsync(Socket socket)
    {
        var serverSocketOptions = new ServerSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
        };

        var stream = await GetStream(socket);
        var serverSocket = new ServerSocket(stream, serverSocketOptions, _logger);

        return new ServerConnection(serverSocket, _logger);
    }

    private async Task<Stream> GetStream(Socket socket)
    {
        var networkStream = new NetworkStream(socket);

        if (_config.Certificate is null)
            return networkStream;

        var sslStream = new SslStream(networkStream, false);
        await sslStream.AuthenticateAsServerAsync(_config.Certificate);

        return sslStream;
    }
}