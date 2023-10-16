using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Mesh.Server.Models;

public abstract class ConnectionStateBase : IAsyncDisposable
{
    public Guid ConnectionId { get; private set; }

    protected AsyncDisposableBox Disposable;

    private readonly ManualResetEventSlim _gate = new(true);

    protected ConnectionStateBase(ILogger logger)
    {
        Disposable = Annium.Disposable.AsyncBox(logger);
        Disposable += _gate;
    }

    public void SetConnectionId(Guid connectionId)
    {
        if (ConnectionId != default)
            throw new InvalidOperationException("ConnectionId is already set");

        ConnectionId = connectionId;
    }

    public IDisposable Lock()
    {
        _gate.Wait();
        return Annium.Disposable.Create(_gate.Set);
    }

    public async ValueTask DisposeAsync()
    {
        await Disposable.DisposeAsync();
        await DoDisposeAsync();
    }

    protected virtual ValueTask DoDisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private record BindableProperty(PropertyInfo Property, MethodInfo Bind);
}