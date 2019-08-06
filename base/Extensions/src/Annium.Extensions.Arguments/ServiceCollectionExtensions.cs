using Annium.Extensions.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddArguments(this IServiceCollection services)
        {
            services.AddSingleton<IArgumentProcessor, ArgumentProcessor>();
            services.AddSingleton<IConfigurationBuilder, ConfigurationBuilder>();
            services.AddSingleton<IConfigurationProcessor, ConfigurationProcessor>();
            services.AddSingleton<IHelpBuilder, HelpBuilder>();
            services.AddSingleton<Root, Root>();
        }
    }
}