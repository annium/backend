using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal sealed class ClientConnectionFactory : IClientConnectionFactory, IClientConnectionFactory<Socket>
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

    public async Task<IManagedConnection> CreateAsync(Socket context)
    {
        var serverSocketOptions = new ServerSocketOptions
        {
            ConnectionMonitor = _config.ConnectionMonitor,
        };

        var stream = await GetStream(context);
        var serverSocket = new ServerSocket(stream, serverSocketOptions, _logger);

        return new ManagedConnection(serverSocket, _logger);
    }

    private async Task<Stream> GetStream(Socket socket)
    {
        var networkStream = new NetworkStream(socket);

        if (_config.AuthOptions is null)
            return networkStream;

        var sslStream = new SslStream(networkStream, false, _config.AuthOptions.RemoteCertificateValidationCallback, null);
        await sslStream.AuthenticateAsClientAsync(_config.AuthOptions);

        return sslStream;
    }
}