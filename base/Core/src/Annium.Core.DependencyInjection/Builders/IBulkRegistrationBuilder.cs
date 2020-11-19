using System;

namespace Annium.Core.DependencyInjection
{
    public interface IBulkRegistrationBuilderBase : IBulkRegistrationBuilderTarget
    {
        IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate);
    }

    public interface IBulkRegistrationBuilderTarget : IBulkRegistrationBuilderLifetime
    {
        IBulkRegistrationBuilderTarget AsSelf();
        IBulkRegistrationBuilderTarget As(Type serviceType);
        IBulkRegistrationBuilderTarget As<T>();
        IBulkRegistrationBuilderTarget AsSelfKeyed<TKey>(Func<Type, TKey> getKey);
        IBulkRegistrationBuilderTarget AsKeyed<TKey>(Type serviceType, Func<Type, TKey> getKey);
        IBulkRegistrationBuilderTarget AsKeyed<T, TKey>(Func<Type, TKey> getKey);
        IBulkRegistrationBuilderTarget AsSelfFactory();
        IBulkRegistrationBuilderTarget AsFactory(Type serviceType);
        IBulkRegistrationBuilderTarget AsFactory<T>();
        IBulkRegistrationBuilderTarget AsSelfKeyedFactory<TKey>(Func<Type, TKey> getKey);
        IBulkRegistrationBuilderTarget AsKeyedFactory<TKey>(Type serviceType, Func<Type, TKey> getKey);
        IBulkRegistrationBuilderTarget AsKeyedFactory<T, TKey>(Func<Type, TKey> getKey);
    }

    public interface IBulkRegistrationBuilderLifetime
    {
        void Scoped();
        void Singleton();
        void Transient();
    }
}