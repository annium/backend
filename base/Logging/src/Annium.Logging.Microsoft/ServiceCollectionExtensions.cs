using Annium.Logging.Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Annium.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoggingBridge(this IServiceCollection services)
    {
        services.AddScoped<ILoggerProvider, LoggingBridgeProvider>();

        return services;
    }
}