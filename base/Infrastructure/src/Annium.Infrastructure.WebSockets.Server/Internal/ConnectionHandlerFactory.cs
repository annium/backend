using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory<TState>
        where TState : ConnectionStateBase
    {
        private readonly IServerLifetime _lifetime;
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly LifeCycleCoordinator<TState> _lifeCycleCoordinator;
        private readonly Func<Guid, TState> _stateFactory;

        public ConnectionHandlerFactory(
            IServerLifetime lifetime,
            IMediator mediator,
            Serializer serializer,
            LifeCycleCoordinator<TState> lifeCycleCoordinator,
            Func<Guid,TState> stateFactory
        )
        {
            _lifetime = lifetime;
            _mediator = mediator;
            _serializer = serializer;
            _lifeCycleCoordinator = lifeCycleCoordinator;
            _stateFactory = stateFactory;
        }

        public ConnectionHandler<TState> Create(IServiceProvider sp, Connection connection)
        {
            return new(
                _lifetime,
                _mediator,
                _serializer,
                sp.Resolve<WorkScheduler>(),
                _lifeCycleCoordinator,
                _stateFactory,
                connection
            );
        }
    }
}