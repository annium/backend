using System;

namespace Annium.Core.DependencyInjection
{
    public interface IInstanceRegistrationBuilderBase
    {
        IInstanceRegistrationBuilderBase AsSelf();
        IInstanceRegistrationBuilderBase As(Type serviceType);
        IInstanceRegistrationBuilderBase As<T>();
        IInstanceRegistrationBuilderBase AsSelfKeyed<TKey>(TKey key);
        IInstanceRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key);
        IInstanceRegistrationBuilderBase AsKeyed<T, TKey>(TKey key);
        IInstanceRegistrationBuilderBase AsSelfFactory();
        IInstanceRegistrationBuilderBase AsFactory(Type serviceType);
        IInstanceRegistrationBuilderBase AsFactory<T>();
        IInstanceRegistrationBuilderBase AsSelfKeyedFactory<TKey>(TKey key);
        IInstanceRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key);
        IInstanceRegistrationBuilderBase AsKeyedFactory<T, TKey>(TKey key);
    }
}