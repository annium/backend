using Annium.Debug;
using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class TestClientFactory : ITestClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly SerializerFactory _serializerFactory;
    private readonly ILogger _logger;
    private readonly ITracer _tracer;

    public TestClientFactory(
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

    public ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration)
    {
        var serializer = _serializerFactory.Create(configuration);

        return new TestClient(socket, _timeProvider, serializer, configuration, _logger, _tracer);
    }
}