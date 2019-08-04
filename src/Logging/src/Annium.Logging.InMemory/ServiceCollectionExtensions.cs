using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryLogger(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));

            return services;
        }
    }
}