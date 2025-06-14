using System;
using System.Threading;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;

// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Manages the context and lifecycle of a mesh server connection, including connection state, cancellation, and logging.
/// </summary>
public sealed class ConnectionContext : IDisposable, ILogSubject
{
    /// <summary>
    /// Gets the unique identifier for this connection.
    /// </summary>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// Gets the underlying mesh connection for sending and receiving data.
    /// </summary>
    public ISendingReceivingConnection Connection => _connection.NotNull();

    /// <summary>
    /// Gets the cancellation token that is canceled when the connection should be terminated.
    /// </summary>
    public CancellationToken Ct => _cts.NotNull().Token;

    /// <summary>
    /// Gets a value indicating whether this connection context has been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed == 1;

    /// <summary>
    /// The cancellation token source for controlling connection lifecycle.
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    /// The underlying mesh connection for sending and receiving data.
    /// </summary>
    private ISendingReceivingConnection? _connection;

    /// <summary>
    /// Indicates whether this connection context has been disposed (1 = disposed, 0 = not disposed).
    /// </summary>
    private int _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionContext"/> class.
    /// </summary>
    /// <param name="logger">The logger for this connection context.</param>
    public ConnectionContext(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Initializes the connection context with the specified connection details.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection.</param>
    /// <param name="connection">The mesh connection for sending and receiving data.</param>
    /// <param name="cts">The cancellation token source for controlling connection lifecycle.</param>
    public void Init(Guid connectionId, ISendingReceivingConnection connection, CancellationTokenSource cts)
    {
        if (Interlocked.CompareExchange(ref _connection, connection, null) is not null)
            throw new InvalidOperationException("Connection context is already initiated");

        ConnectionId = connectionId;
        _cts = cts;
    }

    /// <summary>
    /// Cancels the connection by requesting cancellation on the internal cancellation token source.
    /// </summary>
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

    /// <summary>
    /// Disposes the connection context, releasing all associated resources.
    /// </summary>
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

    /// <summary>
    /// Gets the logger associated with this connection context.
    /// </summary>
    public ILogger Logger { get; }
}
