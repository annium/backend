using IBuilderBase = Annium.Core.DependencyInjection.IFactoryRegistrationBuilderBase;

namespace Annium.Core.DependencyInjection;

public static class FactoryRegistrationBuilderExtensions
{
    public static IBuilderBase As<T>(this IBuilderBase builder) => builder.As(typeof(T));
    public static IBuilderBase AsKeyed<T, TKey>(this IBuilderBase builder, TKey key) where TKey : notnull => builder.AsKeyed(typeof(T), key);
}