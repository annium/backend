using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public readonly struct ConnectionScope<T1, T2> : IAsyncDisposable
    where T1 : DataConnection
    where T2 : DataConnection
{
    [MemberNotNullWhen(false, nameof(Cn1), nameof(Cn2))]
    public bool IsDisposed { get; init; }

    public T1? Cn1 { get; init; } = null;
    public T2? Cn2 { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public ConnectionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public ConnectionScope(AsyncServiceScope scope, T1 cn1, T2 cn2)
    {
        _scope = scope;
        Cn1 = cn1;
        Cn2 = cn2;
    }

    [MemberNotNull(nameof(Cn1), nameof(Cn2))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    public void Deconstruct(out T1 cn1, out T2 cn2)
    {
        ThrowIfDisposed();
        cn1 = Cn1;
        cn2 = Cn2;
    }

    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        await Cn1.DisposeAsync();
        await Cn2.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
