using System;

namespace Annium.Core.DependencyInjection
{
    public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
    {
        IFactoryRegistrationBuilderBase AsSelf();
        IFactoryRegistrationBuilderBase As(Type serviceType);
        IFactoryRegistrationBuilderBase As<T>();
        IFactoryRegistrationBuilderBase AsSelfKeyed<TKey>(TKey key);
        IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key);
        IFactoryRegistrationBuilderBase AsKeyed<T, TKey>(TKey key);
    }

    public interface IFactoryRegistrationBuilderLifetime
    {
        void Scoped();
        void Singleton();
        void Transient();
    }
}