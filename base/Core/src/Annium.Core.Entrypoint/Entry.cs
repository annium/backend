using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Entrypoint;

public record struct Entry(
    IServiceProvider Provider,
    CancellationToken Ct,
    ManualResetEventSlim _gate
) : IAsyncDisposable
{
    public readonly IServiceProvider Provider = Provider;
    public readonly CancellationToken Ct = Ct;
    private readonly ManualResetEventSlim _gate = _gate;

    public void Deconstruct(
        out IServiceProvider provider,
        out CancellationToken ct
    )
    {
        provider = Provider;
        ct = Ct;
    }

    public async ValueTask DisposeAsync()
    {
        if (Provider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (Provider is IDisposable disposable)
            disposable.Dispose();

        _gate.Set();
    }
}