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

            var validatorBase = typeof(IValidationContainer<>);
            var validatorTypes = typeManager.GetImplementations(validatorBase);
            foreach (var type in validatorTypes)
            {
                var baseType = type.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorBase);

                if (!baseType.ContainsGenericParameters)
                    services.AddScoped(baseType, type);
            }

            services.AddScoped(typeof(IValidator<>), typeof(ValidationExecutor<>));

            return services;
        }
    }
}