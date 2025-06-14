using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Dct = LinqToDB.Data.DataConnectionTransaction;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Represents a scoped database transaction that manages the lifetime of a single DataConnection and its transaction.
/// Implements IAsyncDisposable to ensure proper cleanup with automatic rollback on disposal.
/// </summary>
/// <typeparam name="T">The type of DataConnection to manage</typeparam>
public readonly struct TransactionScope<T> : IAsyncDisposable
    where T : DataConnection
{
    /// <summary>
    /// Gets a value indicating whether this transaction scope has been disposed.
    /// When false, guarantees that Cn and Txn are not null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Cn), nameof(Txn))]
    public bool IsDisposed { get; init; }

    /// <summary>
    /// Gets the managed DataConnection instance. Will be null if the scope is disposed.
    /// </summary>
    public T? Cn { get; init; } = null;

    /// <summary>
    /// Gets the managed DataConnectionTransaction instance. Will be null if the scope is disposed.
    /// </summary>
    public Dct? Txn { get; init; } = null;

    /// <summary>
    /// The underlying service scope that manages the lifetime of the DataConnection and its dependencies
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
    /// Initializes a new instance of the TransactionScope struct with an active connection and transaction.
    /// </summary>
    /// <param name="scope">The service scope that manages the connection's lifetime</param>
    /// <param name="cn">The DataConnection instance to manage</param>
    /// <param name="txn">The DataConnectionTransaction instance to manage</param>
    public TransactionScope(AsyncServiceScope scope, T cn, Dct txn)
    {
        _scope = scope;
        Cn = cn;
        Txn = txn;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this transaction scope has been disposed.
    /// After this method returns normally, Cn and Txn are guaranteed to be non-null.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the transaction scope is disposed</exception>
    [MemberNotNull(nameof(Cn), nameof(Txn))]
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
    /// <param name="cn">The DataConnection instance</param>
    /// <param name="txn">The DataConnectionTransaction instance</param>
    /// <exception cref="ObjectDisposedException">Thrown when the transaction scope is disposed</exception>
    public void Deconstruct(out T cn, out Dct txn)
    {
        ThrowIfDisposed();
        cn = Cn;
        txn = Txn;
    }

    /// <summary>
    /// Asynchronously disposes the managed transaction and connection with automatic rollback.
    /// The transaction is rolled back before disposing the connection and service scope.
    /// Safe to call multiple times - subsequent calls are ignored.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        // cn
        await Txn.RollbackAsync(); // relies on linq2db internal transaction nullifying on commit
        await Cn.DisposeAsync();

        // scope
        await _scope.DisposeAsync();
    }
}
