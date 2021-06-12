using Annium.Core.Runtime.Time;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class ClientFactory : IClientFactory
    {
        private readonly ITimeProvider _timeProvider;
        private readonly SerializerFactory _serializerFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ClientFactory(
            ITimeProvider timeProvider,
            SerializerFactory serializerFactory,
            ILoggerFactory loggerFactory
        )
        {
            _timeProvider = timeProvider;
            _serializerFactory = serializerFactory;
            _loggerFactory = loggerFactory;
        }

        public IClient Create(IClientConfiguration configuration)
        {
            var serializer = _serializerFactory.Create(configuration);

            return new Client(_timeProvider, serializer, configuration, _loggerFactory);
        }
    }
}