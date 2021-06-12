using Annium.Core.Runtime.Time;
using Annium.Logging.Abstractions;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class TestClientFactory : ITestClientFactory
    {
        private readonly ITimeProvider _timeProvider;
        private readonly SerializerFactory _serializerFactory;
        private readonly ILoggerFactory _loggerFactory;

        public TestClientFactory(
            ITimeProvider timeProvider,
            SerializerFactory serializerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _timeProvider = timeProvider;
            _serializerFactory = serializerFactory;
            _loggerFactory = loggerFactory;
        }

        public ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration)
        {
            var serializer = _serializerFactory.Create(configuration);

            return new TestClient(socket, _timeProvider, serializer, configuration, _loggerFactory);
        }
    }
}