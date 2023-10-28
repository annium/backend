using System;
using System.Threading;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal.Models;

internal sealed class ConnectionContext
{
    public Guid ConnectionId { get; private set; }
    public ISendingReceivingConnection Connection => _connection.NotNull();
    public CancellationTokenSource Cts { get; private set; } = default!;
    private ISendingReceivingConnection? _connection;

    public void Init(Guid connectionId, ISendingReceivingConnection connection, CancellationTokenSource cts)
    {
        if (Interlocked.CompareExchange(ref _connection, connection, null) is not null)
            throw new InvalidOperationException("Connection context is already initiated");

        ConnectionId = connectionId;
        Cts = cts;
    }
}
