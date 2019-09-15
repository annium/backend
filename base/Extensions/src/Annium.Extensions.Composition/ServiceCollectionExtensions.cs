using System.Linq;
using Annium.Core.Reflection;
using Annium.Extensions.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComposition(this IServiceCollection services)
        {
            var typeManager = TypeManager.Instance;

            var composerBase = typeof(ICompositionContainer<>);
            var composerTypes = typeManager.GetImplementations(composerBase);
            foreach (var type in composerTypes)
            {
                var baseType = type.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == composerBase);

                if (!baseType.ContainsGenericParameters)
                    services.AddScoped(baseType, type);
            }

            services.AddScoped(typeof(IComposer<>), typeof(CompositionExecutor<>));

            return services;
        }
    }
}