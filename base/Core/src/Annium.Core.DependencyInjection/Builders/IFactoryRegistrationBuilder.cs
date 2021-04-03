using System;

namespace Annium.Core.DependencyInjection
{
    public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
    {
        IFactoryRegistrationBuilderBase AsSelf();
        IFactoryRegistrationBuilderBase As(Type serviceType);
        IFactoryRegistrationBuilderBase AsInterfaces();
        IFactoryRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull;
        IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull;
    }

    public interface IFactoryRegistrationBuilderLifetime
    {
        void In(ServiceLifetime lifetime);
        void Scoped();
        void Singleton();
        void Transient();
    }
}