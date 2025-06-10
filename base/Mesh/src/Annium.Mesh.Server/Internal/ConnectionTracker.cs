using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Tracks active server connections and manages their lifecycle, ensuring proper cleanup during server shutdown.
/// </summary>
internal class ConnectionTracker : IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// Gets the logger for this connection tracker.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The server lifetime manager.
    /// </summary>
    private readonly IServerLifetime _lifetime;

    /// <summary>
    /// Dictionary of active connections by their unique identifiers.
    /// </summary>
    private readonly Dictionary<Guid, ConnectionRef> _connections = new();

    /// <summary>
    /// Task completion source for signaling when disposal is complete.
    /// </summary>
    private readonly TaskCompletionSource<object> _disposeTcs = new();

    /// <summary>
    /// Flag indicating whether disposal has started.
    /// </summary>
    private bool _isDisposing;

    /// <summary>
    /// Flag indicating whether disposal has completed.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTracker"/> class.
    /// </summary>
    /// <param name="lifetime">The server lifetime manager.</param>
    /// <param name="logger">The logger for this connection tracker.</param>
    public ConnectionTracker(IServerLifetime lifetime, ILogger logger)
    {
        _lifetime = lifetime;
        Logger = logger;
        _lifetime.Stopping.Register(TryStop);
    }

    /// <summary>
    /// Tracks a new server connection and assigns it a unique identifier.
    /// </summary>
    /// <param name="connection">The server connection to track.</param>
    /// <returns>The unique identifier assigned to the connection.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker is disposing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the server is stopping.</exception>
    public Guid Track(IServerConnection connection)
    {
        var id = Guid.NewGuid();

        this.Trace("cn {id} - start", id);

        EnsureNotDisposing();

        if (_lifetime.Stopping.IsCancellationRequested)
            throw new InvalidOperationException("Server is already stopping");

        lock (_connections)
            _connections[id] = new ConnectionRef(id, connection, Logger);

        this.Trace("cn {id} - done", id);

        return id;
    }

    /// <summary>
    /// Attempts to get a tracked connection by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the connection.</param>
    /// <param name="connectionRef">The disposable reference to the connection if found.</param>
    /// <returns>True if the connection was found and acquired, false otherwise.</returns>
    public bool TryGet(
        Guid id,
        [NotNullWhen(true)] out IDisposableReference<ISendingReceivingConnection>? connectionRef
    )
    {
        // not available, if already disposing
        if (_isDisposing)
        {
            this.Trace("cn {id} - unavailable, is disposing", id);
            connectionRef = null;
            return false;
        }

        lock (_connections)
        {
            if (!_connections.TryGetValue(id, out var cnRef))
            {
                this.Trace("cn {id} - missing", id);
                connectionRef = null;
                return false;
            }

            cnRef.Acquire();
            connectionRef = Disposable.Reference(
                cnRef.Connection,
                () =>
                {
                    cnRef.Release(_isDisposing);
                    return Task.CompletedTask;
                }
            );

            return true;
        }
    }

    /// <summary>
    /// Releases a tracked connection and removes it from tracking.
    /// </summary>
    /// <param name="id">The unique identifier of the connection to release.</param>
    /// <returns>A task that represents the asynchronous release operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker is disposed.</exception>
    public async Task ReleaseAsync(Guid id)
    {
        // can be called after disposing starts, but invalid, if already disposed
        EnsureNotDisposed();

        this.Trace("cn {id} - start", id);
        ConnectionRef? cnRef;
        lock (_connections)
        {
            if (!_connections.Remove(id, out cnRef))
            {
                this.Trace("cn {id} - not found", id);
                return;
            }
        }

        this.Trace("cn {id} - try dispose", id);
        cnRef.TryDispose();

        this.Trace("cn {id} - wait until can be released", id);
#pragma warning disable VSTHRD003
        await cnRef.CanBeReleased;
#pragma warning restore VSTHRD003

        if (_lifetime.Stopping.IsCancellationRequested)
        {
            this.Trace("cn {id} - try stop", id);
            TryStop();
        }

        this.Trace("cn {id} - done", id);
    }

    /// <summary>
    /// Gets all active sending connections for broadcasting operations.
    /// </summary>
    /// <returns>A read-only collection of sending connections.</returns>
    public IReadOnlyCollection<ISendingConnection> GetSendingConnections()
    {
        lock (_connections)
            return _connections.Values.Select(x => x.Connection).ToArray();
    }

    /// <summary>
    /// Disposes the connection tracker asynchronously, waiting for all connections to be released.
    /// </summary>
    /// <returns>A value task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        // not ensuring single call, because for some reason is invoked twice from integration tests
        _isDisposing = true;

        this.Trace("start");
#pragma warning disable VSTHRD003
        await _disposeTcs.Task;
#pragma warning restore VSTHRD003
        this.Trace("done");

        _isDisposed = true;
    }

    /// <summary>
    /// Attempts to complete the disposal process if no connections remain.
    /// </summary>
    private void TryStop()
    {
        lock (_connections)
        {
            this.Trace("Unreleased connections: {connectionsCount}", _connections.Count);
            if (_connections.Count == 0)
                _disposeTcs.TrySetResult(new object());
        }
    }

    /// <summary>
    /// Ensures that the tracker is not in the disposing state.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker is disposing.</exception>
    private void EnsureNotDisposing()
    {
        if (_isDisposing)
            throw new ObjectDisposedException(nameof(ConnectionTracker));
    }

    /// <summary>
    /// Ensures that the tracker has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the tracker is disposed.</exception>
    private void EnsureNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ConnectionTracker));
    }

    /// <summary>
    /// Represents a reference-counted connection that can be safely disposed when no longer in use.
    /// </summary>
    /// <param name="Id">The unique identifier of the connection.</param>
    /// <param name="Connection">The server connection instance.</param>
    /// <param name="Logger">The logger for this connection reference.</param>
    private sealed record ConnectionRef(Guid Id, IServerConnection Connection, ILogger Logger) : ILogSubject
    {
        /// <summary>
        /// Gets a task that completes when the connection can be safely released.
        /// </summary>
        public Task CanBeReleased => _disposeTcs.Task;

        /// <summary>
        /// Task completion source for signaling when the connection can be released.
        /// </summary>
        private readonly TaskCompletionSource<object?> _disposeTcs = new();

        /// <summary>
        /// The current reference count for this connection.
        /// </summary>
        private int _refCount;

        /// <summary>
        /// Increments the reference count for this connection.
        /// </summary>
        public void Acquire()
        {
            var count = Interlocked.Increment(ref _refCount);
            this.Trace("cn {id}: {count}", Id, count);
        }

        /// <summary>
        /// Decrements the reference count and attempts disposal if requested and count reaches zero.
        /// </summary>
        /// <param name="tryDispose">Whether to attempt disposal when count reaches zero.</param>
        public void Release(bool tryDispose)
        {
            var count = Interlocked.Decrement(ref _refCount);
            this.Trace("cn {id}: {count} ({tryDispose})", Id, count, tryDispose);
            if (count == 0 && tryDispose)
                _disposeTcs.TrySetResult(null);
        }

        /// <summary>
        /// Attempts to dispose the connection if the reference count is zero.
        /// </summary>
        public void TryDispose()
        {
            var count = Volatile.Read(ref _refCount);
            this.Trace("cn {id}: {count}", Id, count);
            if (count == 0)
                _disposeTcs.TrySetResult(null);
        }
    }
}
