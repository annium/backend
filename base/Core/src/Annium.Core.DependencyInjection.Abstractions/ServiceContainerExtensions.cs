namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static ISingleRegistrationBuilderBase Add<TImplementationType>(this IServiceContainer container) =>
            container.Add(typeof(TImplementationType));

        public static ISingleRegistrationBuilderBase TryAdd<TImplementationType>(this IServiceContainer container) =>
            container.TryAdd(typeof(TImplementationType));
    }
}