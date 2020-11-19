using System;

namespace Annium.Core.DependencyInjection
{
    public interface ISingleRegistrationBuilderBase : ISingleRegistrationBuilderTarget
    {
    }

    public interface ISingleRegistrationBuilderTarget : ISingleRegistrationBuilderConfigure
    {
        ISingleRegistrationBuilderTarget As(Type serviceType);
        ISingleRegistrationBuilderTarget AsFactory(Type serviceType);
    }

    public interface ISingleRegistrationBuilderConfigure : ISingleRegistrationBuilderLifetime
    {
        ISingleRegistrationBuilderConfigure Configure(Action<ISingleRegistrationUnit> configure);
    }

    public interface ISingleRegistrationBuilderLifetime
    {
        ISingleRegistrationUnit Unit { get; }
        void Scoped();
        void Singleton();
        void Transient();
    }
}