using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Testing.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing
{
    public static class AssemblyServicesCollector
    {
        public static IServiceCollection Collect(Assembly assembly, IEnumerable<Test> tests)
        {
            var services = new ServiceCollection();

            // fixtures
            foreach (var type in assembly.GetTypes().Where(t => t.GetTypeInfo().GetCustomAttribute<FixtureAttribute>() != null))
                services.AddTransient(type);

            // test classes
            foreach (var type in tests.Select(t => t.Method.DeclaringType!).Distinct())
                services.AddTransient(type);

            return services;
        }
    }
}