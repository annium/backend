using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Dct = LinqToDB.Data.DataConnectionTransaction;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public readonly struct TransactionScope<T1, T2> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
{
    [MemberNotNullWhen(false, nameof(Cn1), nameof(Txn1), nameof(Cn2), nameof(Txn2))]
    public bool IsDisposed { get; init; }

    public T1? Cn1 { get; init; } = null;
    public Dct? Txn1 { get; init; } = null;
    public T2? Cn2 { get; init; } = null;
    public Dct? Txn2 { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public TransactionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public TransactionScope(AsyncServiceScope scope, T1 cn1, Dct txn1, T2 cn2, Dct txn2)
    {
        _scope = scope;
        Cn1 = cn1;
        Txn1 = txn1;
        Cn2 = cn2;
        Txn2 = txn2;
    }

    [MemberNotNull(nameof(Cn1), nameof(Txn1), nameof(Cn2), nameof(Txn2))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    public void Deconstruct(out T1 cn1, out Dct txn1, out T2 cn2, out Dct txn2)
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        txn1 = Txn1;
        cn2 = Cn2;
        txn2 = Txn2;
    }

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
