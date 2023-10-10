using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

internal class ClientFactory : IClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly IClientConnectionFactory _clientConnectionFactory;
    private readonly ISerializer _serializer;
    private readonly ILogger _logger;

    public ClientFactory(
        ITimeProvider timeProvider,
        IClientConnectionFactory clientConnectionFactory,
        ISerializer serializer,
        ILogger logger
    )
    {
        _timeProvider = timeProvider;
        _clientConnectionFactory = clientConnectionFactory;
        _serializer = serializer;
        _logger = logger;
    }

    public IClient Create(IClientConfiguration configuration)
    {
        var connection = _clientConnectionFactory.Create();

        return new Client(connection, _timeProvider, _serializer, configuration, _logger);
    }

    public IManagedClient Create(IManagedConnection connection, IClientConfiguration configuration)
    {
        return new ManagedClient(connection, _timeProvider, _serializer, configuration, _logger);
    }
}