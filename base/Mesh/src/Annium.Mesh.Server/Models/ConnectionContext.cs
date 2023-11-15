using System;
using System.Threading;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Models;

public sealed class ConnectionContext : IDisposable, ILogSubject
{
    public Guid ConnectionId { get; private set; }
    public ISendingReceivingConnection Connection => _connection.NotNull();
    public CancellationToken Ct => _cts.NotNull().Token;
    public bool IsDisposed => _isDisposed == 1;
    private CancellationTokenSource? _cts;
    private ISendingReceivingConnection? _connection;
    private int _isDisposed;

    public ConnectionContext(ILogger logger)
    {
        Logger = logger;
    }

    public void Init(Guid connectionId, ISendingReceivingConnection connection, CancellationTokenSource cts)
    {
        if (Interlocked.CompareExchange(ref _connection, connection, null) is not null)
            throw new InvalidOperationException("Connection context is already initiated");

        ConnectionId = connectionId;
        _cts = cts;
    }

    public void Cancel()
    {
        this.Trace("start");

        var cts = _cts.NotNull();
        if (cts.IsCancellationRequested)
            this.Trace("skip - already canceled");
        else
        {
            this.Trace("cancel cts");
            cts.Cancel();
        }

        this.Trace("done");
    }

    public void Dispose()
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
        {
            this.Trace("skip - already disposed");
            return;
        }

        this.Trace("dispose cts");
        _cts.NotNull().Dispose();
        _connection = null;

        this.Trace("done");
    }

    public ILogger Logger { get; }
}
