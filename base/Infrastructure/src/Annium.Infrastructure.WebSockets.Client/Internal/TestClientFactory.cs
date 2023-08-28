using Annium.Debug;
using Annium.Logging.Abstractions;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class TestClientFactory : ITestClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly SerializerFactory _serializerFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITracer _tracer;

    public TestClientFactory(
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

    public ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration)
    {
        var serializer = _serializerFactory.Create(configuration);

        return new TestClient(socket, _timeProvider, serializer, configuration, _loggerFactory, _tracer);
    }
}