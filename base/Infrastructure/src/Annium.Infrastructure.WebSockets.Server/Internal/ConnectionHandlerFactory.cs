using Annium.Core.Mediator;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory
    {
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly WorkScheduler _scheduler;

        public ConnectionHandlerFactory(
            IMediator mediator,
            Serializer serializer,
            WorkScheduler scheduler
        )
        {
            _mediator = mediator;
            _serializer = serializer;
            _scheduler = scheduler;
        }

        public ConnectionHandler Create(Connection connection)
        {
            return new(_mediator, _serializer, _scheduler, connection);
        }
    }
}