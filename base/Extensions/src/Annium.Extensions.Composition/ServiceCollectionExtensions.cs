using System.Linq;
using Annium.Core.Application.Types;
using Annium.Extensions.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComposition(this IServiceCollection services)
        {
            var typeManager = TypeManager.Instance;

            var baseType = typeof(IComposer<>);
            var composerTypes = typeManager.GetImplementations(baseType);
            foreach (var type in composerTypes)
            {
                var @interface = type.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType);

                if (!@interface.ContainsGenericParameters)
                    services.AddScoped(@interface, type);
            }

            return services;
        }
    }
}