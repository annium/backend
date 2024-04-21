using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Dct = LinqToDB.Data.DataConnectionTransaction;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public readonly struct TransactionScope<T1, T2, T3, T4> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
    where T3 : DataConnection
    where T4 : DataConnection
{
    [MemberNotNullWhen(
        false,
        nameof(Cn1),
        nameof(Txn1),
        nameof(Cn2),
        nameof(Txn2),
        nameof(Cn3),
        nameof(Txn3),
        nameof(Cn4),
        nameof(Txn4)
    )]
    public bool IsDisposed { get; init; }

    public T1? Cn1 { get; init; } = null;
    public Dct? Txn1 { get; init; } = null;
    public T2? Cn2 { get; init; } = null;
    public Dct? Txn2 { get; init; } = null;
    public T3? Cn3 { get; init; } = null;
    public Dct? Txn3 { get; init; } = null;
    public T4? Cn4 { get; init; } = null;
    public Dct? Txn4 { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public TransactionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public TransactionScope(
        AsyncServiceScope scope,
        T1 cn1,
        Dct txn1,
        T2 cn2,
        Dct txn2,
        T3 cn3,
        Dct txn3,
        T4 cn4,
        Dct txn4
    )
    {
        _scope = scope;
        Cn1 = cn1;
        Txn1 = txn1;
        Cn2 = cn2;
        Txn2 = txn2;
        Cn3 = cn3;
        Txn3 = txn3;
        Cn4 = cn4;
        Txn4 = txn4;
    }

    [MemberNotNull(
        nameof(Cn1),
        nameof(Txn1),
        nameof(Cn2),
        nameof(Txn2),
        nameof(Cn3),
        nameof(Txn3),
        nameof(Cn4),
        nameof(Txn4)
    )]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    public void Deconstruct(
        out T1 cn1,
        out Dct txn1,
        out T2 cn2,
        out Dct txn2,
        out T3 cn3,
        out Dct txn3,
        out T4 cn4,
        out Dct txn4
    )
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        txn1 = Txn1;
        cn2 = Cn2;
        txn2 = Txn2;
        cn3 = Cn3;
        txn3 = Txn3;
        cn4 = Cn4;
        txn4 = Txn4;
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

        // cn 3
        await Txn3.RollbackAsync(); // relies on linq2db internal transaction nullifying on commit
        await Cn3.DisposeAsync();

        // cn 4
        await Txn4.RollbackAsync(); // relies on linq2db internal transaction nullifying on commit
        await Cn4.DisposeAsync();

        // scope
        await _scope.DisposeAsync();
    }
}
