using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryLogger(this IServiceCollection services, LoggerConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));

            return services;
        }
    }
}