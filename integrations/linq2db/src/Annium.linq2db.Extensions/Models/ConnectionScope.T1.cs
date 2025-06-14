using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Represents a scoped database connection that manages the lifetime of a single DataConnection instance.
/// Implements IAsyncDisposable to ensure proper cleanup of both the connection and the underlying service scope.
/// </summary>
/// <typeparam name="T">The type of DataConnection to manage</typeparam>
public readonly struct ConnectionScope<T> : IAsyncDisposable
    where T : DataConnection
{
    /// <summary>
    /// Gets a value indicating whether this connection scope has been disposed.
    /// When false, guarantees that Cn is not null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Cn))]
    public bool IsDisposed { get; init; }

    /// <summary>
    /// Gets the managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T? Cn { get; init; } = null;

    /// <summary>
    /// The underlying service scope that manages the lifetime of the DataConnection and its dependencies
    /// </summary>
    private readonly AsyncServiceScope _scope;

    /// <summary>
    /// Initializes a new instance of the ConnectionScope struct in a disposed state.
    /// </summary>
    /// <param name="isDisposed">Must be true to create a disposed scope</param>
    public ConnectionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    /// <summary>
    /// Initializes a new instance of the ConnectionScope struct with an active connection.
    /// </summary>
    /// <param name="scope">The service scope that manages the connection's lifetime</param>
    /// <param name="cn">The DataConnection instance to manage</param>
    public ConnectionScope(AsyncServiceScope scope, T cn)
    {
        _scope = scope;
        Cn = cn;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this connection scope has been disposed.
    /// After this method returns normally, Cn is guaranteed to be non-null.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the connection scope is disposed</exception>
    [MemberNotNull(nameof(Cn))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    /// <summary>
    /// Asynchronously disposes the managed connection and service scope.
    /// Safe to call multiple times - subsequent calls are ignored.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        await Cn.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
