using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddAllTypes()
                .AssignableTo(typeof(Validator<>))
                .Where(x => !x.IsGenericType)
                .As(typeof(IValidationContainer<>))
                .InstancePerScope();

            services.AddScoped(typeof(IValidator<>), typeof(ValidationExecutor<>));

            return services;
        }
    }
}