using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory
    {
        private readonly IServerLifetime _lifetime;
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly WorkScheduler _scheduler;

        public ConnectionHandlerFactory(
            IServerLifetime lifetime,
            IMediator mediator,
            Serializer serializer,
            WorkScheduler scheduler
        )
        {
            _lifetime = lifetime;
            _mediator = mediator;
            _serializer = serializer;
            _scheduler = scheduler;
        }

        public ConnectionHandler Create(Connection connection)
        {
            return new(_lifetime, _mediator, _serializer, _scheduler, connection);
        }
    }
}