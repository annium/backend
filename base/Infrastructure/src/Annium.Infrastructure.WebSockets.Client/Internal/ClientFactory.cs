using Annium.Logging;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class ClientFactory : IClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly SerializerFactory _serializerFactory;
    private readonly ILogger _logger;

    public ClientFactory(
        ITimeProvider timeProvider,
        SerializerFactory serializerFactory,
        ILogger logger
    )
    {
        _timeProvider = timeProvider;
        _serializerFactory = serializerFactory;
        _logger = logger;
    }

    public IClient Create(IClientConfiguration configuration)
    {
        var serializer = _serializerFactory.Create(configuration);

        return new Client(_timeProvider, serializer, configuration, _logger);
    }
}