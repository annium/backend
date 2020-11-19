using System;

namespace Annium.Core.DependencyInjection
{
    public interface IInstanceRegistrationBuilderBase : IInstanceRegistrationBuilderTarget
    {
    }

    public interface IInstanceRegistrationBuilderTarget : IInstanceRegistrationBuilderConfigure
    {
        IInstanceRegistrationBuilderTarget As(Type serviceType);
        IInstanceRegistrationBuilderTarget AsFactory(Type serviceType);
    }

    public interface IInstanceRegistrationBuilderConfigure : IInstanceRegistrationBuilderLifetime
    {
        IInstanceRegistrationBuilderConfigure Configure(Action<IInstanceRegistrationUnit> configure);
    }

    public interface IInstanceRegistrationBuilderLifetime
    {
        IInstanceRegistrationUnit Unit { get; }
        void Scoped();
        void Singleton();
        void Transient();
    }
}