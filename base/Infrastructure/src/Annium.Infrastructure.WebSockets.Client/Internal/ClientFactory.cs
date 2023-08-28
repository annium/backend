using Annium.Debug;
using Annium.Logging;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class ClientFactory : IClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly SerializerFactory _serializerFactory;
    private readonly ILogger _logger;
    private readonly ITracer _tracer;

    public ClientFactory(
        ITimeProvider timeProvider,
        SerializerFactory serializerFactory,
        ILogger logger,
        ITracer tracer
    )
    {
        _timeProvider = timeProvider;
        _serializerFactory = serializerFactory;
        _logger = logger;
        _tracer = tracer;
    }

    public IClient Create(IClientConfiguration configuration)
    {
        var serializer = _serializerFactory.Create(configuration);

        return new Client(_timeProvider, serializer, configuration, _logger, _tracer);
    }
}