using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Configuration.Abstractions
{
    public static class Configurator
    {
        public static T Get<T>(
            Action<IConfigurationContainer> configure,
            bool tryLoadReferences
        )
            where T : class, new()
        {
            var services = GetServices<T>(tryLoadReferences);

            services.AddConfiguration<T>(configure);

            return Get<T>(services);
        }

        public static async Task<T> Get<T>(
            Func<IConfigurationContainer, Task> configure,
            bool tryLoadReferences
        )
            where T : class, new()
        {
            var services = GetServices<T>(tryLoadReferences);

            await services.AddConfiguration<T>(configure);

            return Get<T>(services);
        }

        private static IServiceCollection GetServices<T>(bool tryLoadReferences)
        {
            var services = new ServiceCollection();

            services.AddRuntimeTools(typeof(T).Assembly, tryLoadReferences);
            services.AddMapper();

            return services;
        }

        private static T Get<T>(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();

            var configuration = provider.GetRequiredService<T>();

            return configuration;
        }
    }
}