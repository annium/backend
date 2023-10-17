using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class ConnectionHandlerFactory
{
    private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
    private readonly ISerializer _serializer;
    private readonly ILogger _logger;

    public ConnectionHandlerFactory(
        IEnumerable<IConnectionBoundStore> connectionBoundStores,
        ISerializer serializer,
        ILogger logger
    )
    {
        _connectionBoundStores = connectionBoundStores;
        _serializer = serializer;
        _logger = logger;
    }

    public ConnectionHandler Create(IServiceProvider sp, IServerConnection connection)
    {
        return new(
            sp,
            _connectionBoundStores,
            connection,
            _serializer,
            _logger
        );
    }
}