using System;
using System.Reflection;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

namespace Annium.Configuration.Tests
{
    public static class Helper
    {
        public static IServiceProvider GetProvider<T>(
            Action<IConfigurationContainer> configure
        )
            where T : class, new()
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(Assembly.GetCallingAssembly(), false, typeof(Helper).Assembly.ShortName());
            container.AddMapper();
            container.AddConfiguration<T>(configure);

            var provider = container.BuildServiceProvider();

            return provider;
        }
    }
}