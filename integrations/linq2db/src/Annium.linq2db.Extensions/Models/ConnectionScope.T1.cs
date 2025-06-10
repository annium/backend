using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.linq2db.Extensions.Models;

public readonly struct ConnectionScope<T> : IAsyncDisposable
    where T : DataConnection
{
    [MemberNotNullWhen(false, nameof(Cn))]
    public bool IsDisposed { get; init; }

    public T? Cn { get; init; } = null;

    private readonly AsyncServiceScope _scope;

    public ConnectionScope(bool isDisposed)
    {
        IsDisposed = isDisposed;
    }

    public ConnectionScope(AsyncServiceScope scope, T cn)
    {
        _scope = scope;
        Cn = cn;
    }

    [MemberNotNull(nameof(Cn))]
    public void ThrowIfDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(
                nameof(IServiceProvider),
                "Service provider was disposed before creating connection scope"
            );
    }

    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
            return;

        await Cn.DisposeAsync();
        await _scope.DisposeAsync();
    }
}
