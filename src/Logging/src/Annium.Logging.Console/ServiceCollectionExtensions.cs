using Annium.Logging.Abstractions;
using Annium.Logging.Console;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsole(this IServiceCollection services, LoggerConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(typeof(ILogger<>), typeof(ConsoleLogger<>));

            return services;
        }
    }
}