using Annium.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddYamlLocalization(this IServiceCollection services)
        {
            services.AddScoped(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
            services.AddSingleton<IStringLocalizerFactory, YamlStringLocalizerFactory>();

            return services;
        }
    }
}