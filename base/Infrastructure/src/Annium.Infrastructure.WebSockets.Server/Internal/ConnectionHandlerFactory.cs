using System;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory<TState>
        where TState : ConnectionStateBase
    {
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly Func<Guid, TState> _stateFactory;

        public ConnectionHandlerFactory(
            IMediator mediator,
            Serializer serializer,
            Func<Guid,TState> stateFactory
        )
        {
            _mediator = mediator;
            _serializer = serializer;
            _stateFactory = stateFactory;
        }

        public ConnectionHandler<TState> Create(IServiceProvider sp, Connection connection)
        {
            return new(
                sp,
                _mediator,
                _serializer,
                _stateFactory,
                connection
            );
        }
    }
}