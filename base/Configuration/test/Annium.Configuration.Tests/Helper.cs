using System;
using System.Reflection;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Configuration.Tests
{
    public static class Helper
    {
        public static IServiceProvider GetProvider<T>(
            Action<IConfigurationContainer> configure
        )
            where T : class, new()
        {
            var services = new ServiceCollection();
            services.AddRuntimeTools(Assembly.GetCallingAssembly(), false);
            services.AddMapper();
            services.AddConfiguration<T>(configure);

            var provider = services.BuildServiceProvider();

            return provider;
        }
    }
}