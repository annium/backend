using Annium.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            services.AddSingleton<IStorageFactory, StorageFactory>();

            return services;
        }
    }
}