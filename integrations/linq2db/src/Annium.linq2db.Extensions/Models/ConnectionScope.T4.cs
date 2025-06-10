using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Models;

public readonly struct ConnectionScope<T1, T2, T3, T4> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
    where T3 : DataConnection
    where T4 : DataConnection
{
    [MemberNotNullWhen(false, nameof(Cn1), nameof(Cn2), nameof(Cn3), nameof(Cn4))]
    public bool IsDisposed { get; init; }

    public T1? Cn1 { get; init; } = null;
    public T2? Cn2 { get; init; } = null;
    public T3? Cn3 { get; init; } = null;
    public T4? Cn4 { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public ConnectionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public ConnectionScope(AsyncServiceScope scope, T1 cn1, T2 cn2, T3 cn3, T4 cn4)
    {
        _scope = scope;
        Cn1 = cn1;
        Cn2 = cn2;
        Cn3 = cn3;
        Cn4 = cn4;
    }

    [MemberNotNull(nameof(Cn1), nameof(Cn2), nameof(Cn3), nameof(Cn4))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            if (IsDisposed)
                throw new ObjectDisposedException(
                    nameof(IServiceProvider),
                    "Service provider was disposed before creating connection scope"
                );
    }

    public void Deconstruct(out T1 cn1, out T2 cn2, out T3 cn3, out T4 cn4)
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        cn2 = Cn2;
        cn3 = Cn3;
        cn4 = Cn4;
    }

    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        await Cn1.DisposeAsync();
        await Cn2.DisposeAsync();
        await Cn3.DisposeAsync();
        await Cn4.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
