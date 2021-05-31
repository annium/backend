using System;
using System.Collections.Generic;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory<TState>
        where TState : ConnectionStateBase
    {
        private readonly Func<Guid, TState> _stateFactory;
        private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;

        public ConnectionHandlerFactory(
            Func<Guid, TState> stateFactory,
            IEnumerable<IConnectionBoundStore> connectionBoundStores
        )
        {
            _stateFactory = stateFactory;
            _connectionBoundStores = connectionBoundStores;
        }

        public ConnectionHandler<TState> Create(IServiceProvider sp, Connection connection)
        {
            return new(
                sp,
                _stateFactory,
                _connectionBoundStores,
                connection
            );
        }
    }
}