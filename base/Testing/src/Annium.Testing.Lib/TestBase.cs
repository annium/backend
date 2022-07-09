using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Internal;

namespace Annium.Testing.Lib;

public abstract class TestBase
{
    private bool _isBuilt;
    private readonly IServiceProviderBuilder _builder;
    private readonly Lazy<IServiceProvider> _sp;

    protected TestBase()
    {
        Log.SetTestMode();

        _builder = new ServiceProviderFactory().CreateBuilder(new ServiceContainer().Collection);

        Register(SharedRegister);
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

    protected T Get<T>()
        where T : notnull
        => _sp.Value.Resolve<T>();

    protected T GetKeyed<TKey, T>(TKey key)
        where TKey : notnull
        where T : notnull
        => _sp.Value.ResolveKeyed<TKey, T>(key);

    private void SharedRegister(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging();
        container.AddMapper();
    }

    private void SharedSetup(IServiceProvider sp)
    {
        sp.UseLogging(x => x.UseTestConsole());
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