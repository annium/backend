using Annium.Debug;
using Annium.Logging;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class ClientFactory : IClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly SerializerFactory _serializerFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITracer _tracer;

    public ClientFactory(
        ITimeProvider timeProvider,
        SerializerFactory serializerFactory,
        ILoggerFactory loggerFactory,
        ITracer tracer
    )
    {
        _timeProvider = timeProvider;
        _serializerFactory = serializerFactory;
        _loggerFactory = loggerFactory;
        _tracer = tracer;
    }

    public IClient Create(IClientConfiguration configuration)
    {
        var serializer = _serializerFactory.Create(configuration);

        return new Client(_timeProvider, serializer, configuration, _loggerFactory, _tracer);
    }
}