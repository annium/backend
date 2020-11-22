using System;

namespace Annium.Core.DependencyInjection
{
    public interface ISingleRegistrationBuilderBase : ISingleRegistrationBuilderLifetime
    {
        ISingleRegistrationBuilderBase AsSelf();
        ISingleRegistrationBuilderBase As(Type serviceType);
        ISingleRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull;
        ISingleRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull;
        ISingleRegistrationBuilderBase AsSelfFactory();
        ISingleRegistrationBuilderBase AsFactory(Type serviceType);
        ISingleRegistrationBuilderBase AsKeyedSelfFactory<TKey>(TKey key) where TKey : notnull;
        ISingleRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key) where TKey : notnull;
    }

    public interface ISingleRegistrationBuilderLifetime
    {
        void Scoped();
        void Singleton();
        void Transient();
    }
}