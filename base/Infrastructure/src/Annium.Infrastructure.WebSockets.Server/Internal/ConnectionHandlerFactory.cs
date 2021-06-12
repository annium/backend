using System;
using System.Collections.Generic;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandlerFactory<TState>
        where TState : ConnectionStateBase
    {
        private readonly Func<Guid, TState> _stateFactory;
        private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
        private readonly ILogger<ConnectionHandler<TState>> _logger;

        public ConnectionHandlerFactory(
            Func<Guid, TState> stateFactory,
            IEnumerable<IConnectionBoundStore> connectionBoundStores,
            ILogger<ConnectionHandler<TState>> logger
        )
        {
            _stateFactory = stateFactory;
            _connectionBoundStores = connectionBoundStores;
            _logger = logger;
        }

        public ConnectionHandler<TState> Create(IServiceProvider sp, Connection connection)
        {
            return new(
                sp,
                _stateFactory,
                _connectionBoundStores,
                connection,
                _logger
            );
        }
    }
}