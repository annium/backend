using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Debug;

namespace Annium.Core.Entrypoint;

public readonly record struct Entry(
    IServiceProvider Provider,
    CancellationToken Ct,
    ManualResetEventSlim _gate
) : ITraceSubject, IAsyncDisposable
{
    public ITracer Tracer { get; } = Provider.Resolve<ITracer>();
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
        this.Trace("start");

        if (Provider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (Provider is IDisposable disposable)
            disposable.Dispose();

        this.Trace("set gate");

        _gate.Set();

        this.Trace("done");
    }
}