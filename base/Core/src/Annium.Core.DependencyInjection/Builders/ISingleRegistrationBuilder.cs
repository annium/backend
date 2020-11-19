using System;

namespace Annium.Core.DependencyInjection
{
    public interface ISingleRegistrationBuilderBase : ISingleRegistrationBuilderLifetime
    {
        ISingleRegistrationBuilderBase AsSelf();
        ISingleRegistrationBuilderBase As(Type serviceType);
        ISingleRegistrationBuilderBase As<T>();
        ISingleRegistrationBuilderBase AsSelfKeyed<TKey>(TKey key);
        ISingleRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key);
        ISingleRegistrationBuilderBase AsKeyed<T, TKey>(TKey key);
        ISingleRegistrationBuilderBase AsSelfFactory();
        ISingleRegistrationBuilderBase AsFactory(Type serviceType);
        ISingleRegistrationBuilderBase AsFactory<T>();
        ISingleRegistrationBuilderBase AsSelfKeyedFactory<TKey>(TKey key);
        ISingleRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key);
        ISingleRegistrationBuilderBase AsKeyedFactory<T, TKey>(TKey key);
    }

    public interface ISingleRegistrationBuilderLifetime
    {
        void Scoped();
        void Singleton();
        void Transient();
    }
}