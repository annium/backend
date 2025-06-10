using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Dct = LinqToDB.Data.DataConnectionTransaction;

namespace Annium.linq2db.Extensions.Models;

public readonly struct TransactionScope<T> : IAsyncDisposable
    where T : DataConnection
{
    [MemberNotNullWhen(false, nameof(Cn), nameof(Txn))]
    public bool IsDisposed { get; init; }

    public T? Cn { get; init; } = null;
    public Dct? Txn { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public TransactionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public TransactionScope(AsyncServiceScope scope, T cn, Dct txn)
    {
        _scope = scope;
        Cn = cn;
        Txn = txn;
    }

    [MemberNotNull(nameof(Cn), nameof(Txn))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    public void Deconstruct(out T cn, out Dct txn)
    {
        ThrowIfDisposed();
        cn = Cn;
        txn = Txn;
    }

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
