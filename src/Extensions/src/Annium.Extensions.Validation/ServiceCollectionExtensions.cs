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

            var baseType = typeof(Validator<>);
            var validatorTypes = typeManager.GetImplementations(baseType);
            foreach (var implementationType in validatorTypes)
            {
                var serviceType = implementationType;
                while (!serviceType.IsGenericType || serviceType.GetGenericTypeDefinition() != baseType)
                    serviceType = serviceType.BaseType;

                services.AddSingleton(serviceType, implementationType);
            }

            return services;
        }
    }
}