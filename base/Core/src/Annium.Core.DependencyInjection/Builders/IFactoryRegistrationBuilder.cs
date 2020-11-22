using System;

namespace Annium.Core.DependencyInjection
{
    public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
    {
        IFactoryRegistrationBuilderBase AsSelf();
        IFactoryRegistrationBuilderBase As(Type serviceType);
        IFactoryRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull;
        IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull;
    }

    public interface IFactoryRegistrationBuilderLifetime
    {
        void Scoped();
        void Singleton();
        void Transient();
    }
}