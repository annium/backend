using Annium.Extensions.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShell(this IServiceCollection services)
        {
            services.AddSingleton<IShell, Shell>();

            return services;
        }
    }
}