using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

internal class ValueContainer<TState, TValueLoader, TConfig, TValue> : ValueContainerBase<TState, TValue>, IValueContainer<TState, TConfig, TValue>
    where TState : ConnectionStateBase
    where TValueLoader : IValueLoader<TState, TConfig, TValue>
{
    private bool _isConfigured;
    private TConfig? _config;

    public ValueContainer(
        IServiceProvider sp
    ) : base(sp)
    {
    }

    public void Configure(TConfig config)
    {
        if (_isConfigured)
            throw new InvalidOperationException("Container already configured");
        _isConfigured = true;

        _config = config;
    }

    protected override async Task<TValue> LoadValueAsync(IAsyncServiceScope scope)
    {
        if (!_isConfigured)
            throw new InvalidOperationException("Container is not configured");
        var config = _config!;

        var loader = scope.ServiceProvider.Resolve<TValueLoader>();
        var value = await loader.LoadAsync(State!, config);

        return value;
    }
}

internal class ValueContainer<TState, TValueLoader, TValue> : ValueContainerBase<TState, TValue>, IValueContainer<TState, TValue>
    where TState : ConnectionStateBase
    where TValueLoader : IValueLoader<TState, TValue>
{
    public ValueContainer(
        IServiceProvider sp
    ) : base(sp)
    {
    }

    protected override async Task<TValue> LoadValueAsync(IAsyncServiceScope scope)
    {
        var loader = scope.ServiceProvider.Resolve<TValueLoader>();
        var value = await loader.LoadAsync(State!);

        return value;
    }
}

internal abstract class ValueContainerBase<TState, TValue> : IAsyncDisposable
    where TState : ConnectionStateBase
{
    public TValue Value
    {
        get
        {
            EnsureReady();
            return _value;
        }
    }

    public event Action<TValue> OnChange = delegate { };

    private readonly IServiceProvider _sp;
    private readonly AsyncLazy<TValue> _initiator;
    protected TState? State;
    private TValue _value;
    private AsyncDisposableBox _disposable;

    protected ValueContainerBase(
        IServiceProvider sp
    )
    {
        _sp = sp;
        _initiator = new AsyncLazy<TValue>((Func<Task<TValue>>)LoadValueAsync);
        _value = default!;
        _disposable = Disposable.AsyncBox(sp.Resolve<ILogger>());
    }

    public void Bind(TState state)
    {
        if (State is not null)
            throw new InvalidOperationException("Container is already bound to state");
        State = state;
    }

    public void Set(TValue value)
    {
        EnsureReady();

        _value = value;
        OnChange.Invoke(value);
    }


    public TaskAwaiter<TValue> GetAwaiter()
    {
        EnsureBound();

        return _initiator.IsValueCreated
            ? Task.FromResult(_value).GetAwaiter()
            : _initiator.GetAwaiter();
    }

    protected abstract Task<TValue> LoadValueAsync(IAsyncServiceScope scope);

    private async Task<TValue> LoadValueAsync()
    {
        var scope = _sp.CreateAsyncScope();
        _disposable += scope;

        var value = await LoadValueAsync(scope);

        _value = value;
        OnChange.Invoke(value);

        return value;
    }

    private void EnsureReady()
    {
        EnsureBound();

        if (!_initiator.IsValueCreated)
            throw new InvalidOperationException("Container is not initiated");
    }

    private void EnsureBound()
    {
        if (State is null)
            throw new InvalidOperationException("Container is not bound to state");
    }

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}