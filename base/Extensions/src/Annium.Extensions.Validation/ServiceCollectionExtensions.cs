using Annium.Extensions.Validation;
using Annium.Extensions.Validation.Internal;
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
                .AsImplementedInterfaces()
                .InstancePerScope();

            services.AddScoped(typeof(IValidator<>), typeof(ValidationExecutor<>));

            return services;
        }
    }
}