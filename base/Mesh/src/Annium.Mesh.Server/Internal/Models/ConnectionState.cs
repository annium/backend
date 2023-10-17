using System;

namespace Annium.Mesh.Server.Internal.Models;

public sealed class ConnectionState
{
    public Guid ConnectionId { get; private set; }

    public void SetConnectionId(Guid connectionId)
    {
        if (ConnectionId != default)
            throw new InvalidOperationException("ConnectionId is already set");

        ConnectionId = connectionId;
    }
}