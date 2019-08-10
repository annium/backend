using System.Linq;
using Annium.Core.Application.Types;
using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            var typeManager = TypeManager.Instance;

            var baseType = typeof(IValidator<>);
            var validatorTypes = typeManager.GetImplementations(baseType);
            foreach (var type in validatorTypes)
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