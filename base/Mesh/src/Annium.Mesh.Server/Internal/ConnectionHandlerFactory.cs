using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class ConnectionHandlerFactory<TState>
    where TState : ConnectionStateBase
{
    private readonly Func<Guid, TState> _stateFactory;
    private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
    private readonly ISerializer _serializer;
    private readonly ILogger _logger;

    public ConnectionHandlerFactory(
        Func<Guid, TState> stateFactory,
        IEnumerable<IConnectionBoundStore> connectionBoundStores,
        ISerializer serializer,
        ILogger logger
    )
    {
        _stateFactory = stateFactory;
        _connectionBoundStores = connectionBoundStores;
        _serializer = serializer;
        _logger = logger;
    }

    public ConnectionHandler<TState> Create(IServiceProvider sp, IServerConnection connection)
    {
        return new(
            sp,
            _stateFactory,
            _connectionBoundStores,
            connection,
            _serializer,
            _logger
        );
    }
}