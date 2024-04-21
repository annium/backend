using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public readonly struct ConnectionScope<T1, T2, T3> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
    where T3 : DataConnection
{
    [MemberNotNullWhen(false, nameof(Cn1), nameof(Cn2), nameof(Cn3))]
    public bool IsDisposed { get; init; }

    public T1? Cn1 { get; init; } = null;
    public T2? Cn2 { get; init; } = null;
    public T3? Cn3 { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public ConnectionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public ConnectionScope(AsyncServiceScope scope, T1 cn1, T2 cn2, T3 cn3)
    {
        _scope = scope;
        Cn1 = cn1;
        Cn2 = cn2;
        Cn3 = cn3;
    }

    [MemberNotNull(nameof(Cn1), nameof(Cn2), nameof(Cn3))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    public void Deconstruct(out T1 cn1, out T2 cn2, out T3 cn3)
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        cn2 = Cn2;
        cn3 = Cn3;
    }

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
