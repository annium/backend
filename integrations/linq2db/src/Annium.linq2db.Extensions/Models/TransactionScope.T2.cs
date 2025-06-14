using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Dct = LinqToDB.Data.DataConnectionTransaction;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Represents a scoped database transaction that manages the lifetime of two DataConnections and their transactions.
/// Implements IAsyncDisposable to ensure proper cleanup with automatic rollback on disposal.
/// </summary>
/// <typeparam name="T1">The type of the first DataConnection to manage</typeparam>
/// <typeparam name="T2">The type of the second DataConnection to manage</typeparam>
public readonly struct TransactionScope<T1, T2> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
{
    /// <summary>
    /// Gets a value indicating whether this transaction scope has been disposed.
    /// When false, guarantees that Cn1, Txn1, Cn2, and Txn2 are not null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Cn1), nameof(Txn1), nameof(Cn2), nameof(Txn2))]
    public bool IsDisposed { get; init; }

    /// <summary>
    /// Gets the first managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T1? Cn1 { get; init; } = null;

    /// <summary>
    /// Gets the first managed DataConnectionTransaction instance. Will be null if the scope is disposed.
    /// </summary>
    public Dct? Txn1 { get; init; } = null;

    /// <summary>
    /// Gets the second managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T2? Cn2 { get; init; } = null;

    /// <summary>
    /// Gets the second managed DataConnectionTransaction instance. Will be null if the scope is disposed.
    /// </summary>
    public Dct? Txn2 { get; init; } = null;

    /// <summary>
    /// The underlying service scope that manages the lifetime of both DataConnections and their dependencies
    /// </summary>
    private readonly AsyncServiceScope _scope;

    /// <summary>
    /// Initializes a new instance of the TransactionScope struct in a disposed state.
    /// </summary>
    /// <param name="isDisposed">Must be true to create a disposed scope</param>
    public TransactionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    /// <summary>
    /// Initializes a new instance of the TransactionScope struct with active connections and transactions.
    /// </summary>
    /// <param name="scope">The service scope that manages the connections' lifetime</param>
    /// <param name="cn1">The first DataConnection instance to manage</param>
    /// <param name="txn1">The first DataConnectionTransaction instance to manage</param>
    /// <param name="cn2">The second DataConnection instance to manage</param>
    /// <param name="txn2">The second DataConnectionTransaction instance to manage</param>
    public TransactionScope(AsyncServiceScope scope, T1 cn1, Dct txn1, T2 cn2, Dct txn2)
    {
        _scope = scope;
        Cn1 = cn1;
        Txn1 = txn1;
        Cn2 = cn2;
        Txn2 = txn2;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this transaction scope has been disposed.
    /// After this method returns normally, Cn1, Txn1, Cn2, and Txn2 are guaranteed to be non-null.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the transaction scope is disposed</exception>
    [MemberNotNull(nameof(Cn1), nameof(Txn1), nameof(Cn2), nameof(Txn2))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    /// <summary>
    /// Deconstructs the transaction scope into its connection and transaction components.
    /// </summary>
    /// <param name="cn1">The first DataConnection instance</param>
    /// <param name="txn1">The first DataConnectionTransaction instance</param>
    /// <param name="cn2">The second DataConnection instance</param>
    /// <param name="txn2">The second DataConnectionTransaction instance</param>
    /// <exception cref="ObjectDisposedException">Thrown when the transaction scope is disposed</exception>
    public void Deconstruct(out T1 cn1, out Dct txn1, out T2 cn2, out Dct txn2)
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        txn1 = Txn1;
        cn2 = Cn2;
        txn2 = Txn2;
    }

    /// <summary>
    /// Asynchronously disposes both managed transactions and connections with automatic rollback.
    /// All transactions are rolled back before disposing their connections and the service scope.
    /// Safe to call multiple times - subsequent calls are ignored.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        // cn 1
        await Txn1.RollbackAsync(); // relies on linq2db internal transaction nullifying on commit
        await Cn1.DisposeAsync();

        // cn 2
        await Txn2.RollbackAsync(); // relies on linq2db internal transaction nullifying on commit
        await Cn2.DisposeAsync();

        // scope
        await _scope.DisposeAsync();
    }
}
