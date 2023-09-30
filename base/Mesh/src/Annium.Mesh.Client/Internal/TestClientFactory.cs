using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Mesh.Client.Internal;

internal class TestClientFactory : ITestClientFactory
{
    private readonly ITimeProvider _timeProvider;
    private readonly Serializer _serializer;
    private readonly ILogger _logger;

    public TestClientFactory(
        ITimeProvider timeProvider,
        Serializer serializer,
        ILogger logger
    )
    {
        _timeProvider = timeProvider;
        _serializer = serializer;
        _logger = logger;
    }

    public ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration)
    {
        return new TestClient(socket, _timeProvider, _serializer, configuration, _logger);
    }
}