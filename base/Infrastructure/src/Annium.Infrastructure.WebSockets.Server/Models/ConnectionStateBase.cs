using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Reflection;

namespace Annium.Infrastructure.WebSockets.Server.Models;

public abstract class ConnectionStateBase : IAsyncDisposable
{
    private static readonly ConcurrentDictionary<Type, BindableProperty[]> BindableProperties = new();

    public Guid ConnectionId { get; private set; }

    protected AsyncDisposableBox Disposable = Annium.Disposable.AsyncBox();

    private readonly ManualResetEventSlim _gate = new(true);

    protected ConnectionStateBase()
    {
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

    public void BindValues()
    {
        var bindableProperties = BindableProperties.GetOrAdd(
            GetType(),
            type => type
                .GetAllProperties()
                .Where(x => x.PropertyType.GetTargetImplementation(typeof(IValueContainer<,>)) is not null)
                .Select(x => new BindableProperty(
                    x,
                    x.PropertyType.GetMethod(nameof(IValueContainer<ConnectionStateBase, object>.Bind))!
                ))
                .ToArray()
        );

        foreach (var (property, bind) in bindableProperties)
        {
            var container = (IAsyncDisposable) property.GetValue(this)!;
            bind.Invoke(container, new object[] { this });
            Disposable += container;
        }
    }

    protected virtual ValueTask DoDisposeAsync()
    {
        return new();
    }

    private record BindableProperty(PropertyInfo Property, MethodInfo Bind);
}