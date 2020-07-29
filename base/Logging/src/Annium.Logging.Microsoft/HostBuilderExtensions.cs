using Annium.Logging.Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureLoggingBridge(
            this IHostBuilder builder
        )
        {
            return builder
                .ConfigureLogging((ctx, logging) =>
                {
                    logging.ClearProviders();
                    logging.Services.AddScoped<ILoggerProvider, LoggerBridgeProvider>();
                });
        }
    }
}