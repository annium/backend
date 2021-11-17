using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;

namespace Annium.Core.Runtime.Internal.Time;

internal class TimeProviderSwitcher : ITimeProviderSwitcher, ITimeProviderResolver
{
    private TimeType _type;
    private readonly IIndex<TimeType, ITimeProvider> _timeProviders;

    public TimeProviderSwitcher(IIndex<TimeType, ITimeProvider> timeProviders, TimeType defaultType)
    {
        _type = defaultType;
        _timeProviders = timeProviders;
    }

    public void UseRealTime()
        => _type = TimeType.Real;

    public void UseManagedTime()
        => _type = TimeType.Managed;

    public ITimeProvider Resolve()
        => _timeProviders[_type];
}