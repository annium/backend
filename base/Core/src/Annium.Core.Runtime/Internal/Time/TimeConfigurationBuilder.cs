using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;

namespace Annium.Core.Runtime.Internal.Time;

internal class TimeConfigurationBuilder : ITimeConfigurationBuilder
{
    private TimeType? _type;
    private readonly IServiceContainer _container;

    public TimeConfigurationBuilder(IServiceContainer container)
    {
        _container = container;
    }

    public ITimeConfigurationBuilder WithRealTime()
    {
        _container.Add<RealTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Real).Singleton();
        _container.Add<IActionScheduler, ActionScheduler>().Singleton();
        _type = TimeType.Real;

        return this;
    }

    public ITimeConfigurationBuilder WithManagedTime()
    {
        _container.Add<ITimeManager, ManagedTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Managed).Singleton();
        _container.Add<IActionScheduler, ManagedActionScheduler>().Singleton();
        _type = TimeType.Managed;

        return this;
    }

    public IServiceContainer SetDefault()
    {
        if (_type is null)
            throw new InvalidOperationException("Can't set default time provider - no providers registered");

        _container.Add(_type).AsSelf().Singleton();
        _container.Add<TimeProvider>().AsInterfaces().Singleton();

        return _container;
    }
}