using System;
using System.Reflection;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Configuration.Tests
{
    public static class Helper
    {
        public static T BuildConfiguration<T>(
            Action<IConfigurationBuilder> configure
        )
            where T : class, new()
        {
            var services = new ServiceCollection();
            services.AddRuntimeTools(Assembly.GetCallingAssembly(), false);
            services.AddConfiguration<T>(configure);

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<T>();
        }
    }
}