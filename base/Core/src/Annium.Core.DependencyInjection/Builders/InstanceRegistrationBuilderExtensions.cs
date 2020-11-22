using System;
using IBuilderTarget = Annium.Core.DependencyInjection.IInstanceRegistrationBuilderBase;

namespace Annium.Core.DependencyInjection
{
    public static class InstanceRegistrationBuilderExtensions
    {
        public static IBuilderTarget As<T>(this IBuilderTarget builder) =>
            builder.As(typeof(T));

        public static IBuilderTarget AsKeyed<T, TKey>(this IBuilderTarget builder, Func<Type, TKey> getKey) where TKey : notnull =>
            builder.AsKeyed(typeof(T), getKey);

        public static IBuilderTarget AsFactory<T>(this IBuilderTarget builder) =>
            builder.AsFactory(typeof(T));

        public static IBuilderTarget AsKeyedFactory<T, TKey>(this IBuilderTarget builder, Func<Type, TKey> getKey) where TKey : notnull =>
            builder.AsKeyedFactory(typeof(T), getKey);
    }
}