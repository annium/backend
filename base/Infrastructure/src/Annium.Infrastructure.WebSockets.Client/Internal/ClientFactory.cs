using Annium.Core.Runtime.Time;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class ClientFactory : IClientFactory
    {
        private readonly ITimeProvider _timeProvider;
        private readonly SerializerFactory _serializerFactory;

        public ClientFactory(
            ITimeProvider timeProvider,
            SerializerFactory serializerFactory
        )
        {
            _timeProvider = timeProvider;
            _serializerFactory = serializerFactory;
        }

        public IClientBase Create(ClientConfiguration configuration)
        {
            var serializer = _serializerFactory.Create(configuration);

            return new ClientBase(_timeProvider, serializer, configuration);
        }
    }
}