using Annium.Storage.Abstractions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddStorage(this IServiceContainer container)
        {
            container.Add<IStorageFactory, StorageFactory>().Singleton();

            return container;
        }
    }
}