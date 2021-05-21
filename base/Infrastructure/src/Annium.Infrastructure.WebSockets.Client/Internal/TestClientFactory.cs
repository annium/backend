using Annium.Core.Runtime.Time;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class TestClientFactory : ITestClientFactory
    {
        private readonly ITimeProvider _timeProvider;
        private readonly SerializerFactory _serializerFactory;

        public TestClientFactory(
            ITimeProvider timeProvider,
            SerializerFactory serializerFactory
        )
        {
            _timeProvider = timeProvider;
            _serializerFactory = serializerFactory;
        }

        public ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration)
        {
            var serializer = _serializerFactory.Create(configuration);

            return new TestClient(socket, _timeProvider, serializer, configuration);
        }
    }
}