using System;
using Annium.Core.DependencyInjection;
using Annium.Debug;
using Annium.Testing.Lib.Internal;
using Xunit.Abstractions;

namespace Annium.Testing.Lib;

public abstract class TestBase
{
    private bool _isBuilt;
    private readonly IServiceProviderBuilder _builder;
    private readonly Lazy<IServiceProvider> _sp;

    protected TestBase(ITestOutputHelper outputHelper)
    {
        _builder = new ServiceProviderFactory().CreateBuilder(new ServiceContainer().Collection);

        Register(SharedRegister(outputHelper));
        Setup(SharedSetup);

        _sp = new Lazy<IServiceProvider>(BuildServiceProvider, true);
    }

    protected void AddServicePack<T>()
        where T : ServicePackBase, new()
    {
        _builder.UseServicePack<T>();
    }

    protected void Register(Action<IServiceContainer> register)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Register((c, _) => register(c)));
    }

    protected void Setup(Action<IServiceProvider> setup)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Setup(setup));
    }

    protected IAsyncServiceScope CreateAsyncScope()
        => _sp.Value.CreateAsyncScope();

    protected T Get<T>()
        where T : notnull
        => _sp.Value.Resolve<T>();

    protected T GetKeyed<TKey, T>(TKey key)
        where TKey : notnull
        where T : notnull
        => _sp.Value.ResolveKeyed<TKey, T>(key);

    private Action<IServiceContainer> SharedRegister(ITestOutputHelper outputHelper) => container =>
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithManagedTime().WithRelativeTime().SetDefault();
        container.AddLogging();
        container.AddMapper();
        container.Add<ITracer>(new TestTracer(outputHelper)).AsSelf().Singleton();
    };

    private void SharedSetup(IServiceProvider sp)
    {
        sp.UseLogging(x => x.UseConsole());
    }

    private IServiceProvider BuildServiceProvider()
    {
        EnsureNotBuilt();
        _isBuilt = true;

        return _builder.Build();
    }

    private void EnsureNotBuilt()
    {
        if (_isBuilt)
            throw new InvalidOperationException("ServiceProvider is already built");
    }
}