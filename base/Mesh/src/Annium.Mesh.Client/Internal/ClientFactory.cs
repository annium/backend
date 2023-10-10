using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Serialization.Abstractions;
using Constants = Annium.Serialization.Json.Constants;

namespace Annium.Mesh.Client.Internal;

internal class ClientFactory : IClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly IClientConnectionFactory _clientConnectionFactory;
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;
    private readonly ILogger _logger;

    public ClientFactory(
        ITimeProvider timeProvider,
        IClientConnectionFactory clientConnectionFactory,
        IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> serializers,
        ILogger logger
    )
    {
        _timeProvider = timeProvider;
        _clientConnectionFactory = clientConnectionFactory;
        _serializer = serializers[SerializerKey.CreateDefault(Constants.MediaType)];
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