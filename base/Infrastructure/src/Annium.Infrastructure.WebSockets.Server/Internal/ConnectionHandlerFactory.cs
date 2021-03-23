using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory
    {
        private readonly IServerLifetime _lifetime;
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly LifeCycleCoordinator _lifeCycleCoordinator;

        public ConnectionHandlerFactory(
            IServerLifetime lifetime,
            IMediator mediator,
            Serializer serializer,
            LifeCycleCoordinator lifeCycleCoordinator
        )
        {
            _lifetime = lifetime;
            _mediator = mediator;
            _serializer = serializer;
            _lifeCycleCoordinator = lifeCycleCoordinator;
        }

        public ConnectionHandler Create(IServiceProvider sp, Connection connection)
        {
            return new(
                _lifetime,
                _mediator,
                _serializer,
                sp.Resolve<WorkScheduler>(),
                _lifeCycleCoordinator,
                connection
            );
        }
    }
}