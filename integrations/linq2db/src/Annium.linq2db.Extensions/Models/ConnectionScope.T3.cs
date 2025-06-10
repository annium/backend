using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Models;

/// <summary>
/// Represents a scoped database connection that manages the lifetime of three DataConnection instances.
/// Implements IAsyncDisposable to ensure proper cleanup of all connections and the underlying service scope.
/// </summary>
/// <typeparam name="T1">The type of the first DataConnection to manage</typeparam>
/// <typeparam name="T2">The type of the second DataConnection to manage</typeparam>
/// <typeparam name="T3">The type of the third DataConnection to manage</typeparam>
public readonly struct ConnectionScope<T1, T2, T3> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
    where T3 : DataConnection
{
    /// <summary>
    /// Gets a value indicating whether this connection scope has been disposed.
    /// When false, guarantees that Cn1, Cn2, and Cn3 are not null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Cn1), nameof(Cn2), nameof(Cn3))]
    public bool IsDisposed { get; init; }

    /// <summary>
    /// Gets the first managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T1? Cn1 { get; init; } = null;

    /// <summary>
    /// Gets the second managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T2? Cn2 { get; init; } = null;

    /// <summary>
    /// Gets the third managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T3? Cn3 { get; init; } = null;

    /// <summary>
    /// The underlying service scope that manages the lifetime of all three DataConnections and their dependencies
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
    /// Initializes a new instance of the ConnectionScope struct with active connections.
    /// </summary>
    /// <param name="scope">The service scope that manages the connections' lifetime</param>
    /// <param name="cn1">The first DataConnection instance to manage</param>
    /// <param name="cn2">The second DataConnection instance to manage</param>
    /// <param name="cn3">The third DataConnection instance to manage</param>
    public ConnectionScope(AsyncServiceScope scope, T1 cn1, T2 cn2, T3 cn3)
    {
        _scope = scope;
        Cn1 = cn1;
        Cn2 = cn2;
        Cn3 = cn3;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this connection scope has been disposed.
    /// After this method returns normally, Cn1, Cn2, and Cn3 are guaranteed to be non-null.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the connection scope is disposed</exception>
    [MemberNotNull(nameof(Cn1), nameof(Cn2), nameof(Cn3))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    /// <summary>
    /// Deconstructs the connection scope into its individual connection components.
    /// </summary>
    /// <param name="cn1">The first DataConnection instance</param>
    /// <param name="cn2">The second DataConnection instance</param>
    /// <param name="cn3">The third DataConnection instance</param>
    /// <exception cref="ObjectDisposedException">Thrown when the connection scope is disposed</exception>
    public void Deconstruct(out T1 cn1, out T2 cn2, out T3 cn3)
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        cn2 = Cn2;
        cn3 = Cn3;
    }

    /// <summary>
    /// Asynchronously disposes all managed connections and the service scope.
    /// Safe to call multiple times - subsequent calls are ignored.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        await Cn1.DisposeAsync();
        await Cn2.DisposeAsync();
        await Cn3.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
