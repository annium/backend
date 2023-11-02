using System;
using System.Threading;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Models;

public sealed class ConnectionContext : IDisposable
{
    public Guid ConnectionId { get; private set; }
    public ISendingReceivingConnection Connection => _connection.NotNull();
    public CancellationTokenSource Cts { get; private set; } = default!;
    public bool IsDisposed { get; private set; }
    private ISendingReceivingConnection? _connection;

    public void Init(Guid connectionId, ISendingReceivingConnection connection, CancellationTokenSource cts)
    {
        if (Interlocked.CompareExchange(ref _connection, connection, null) is not null)
            throw new InvalidOperationException("Connection context is already initiated");

        ConnectionId = connectionId;
        Cts = cts;
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        Cts.Dispose();
        _connection = null;
    }
}
