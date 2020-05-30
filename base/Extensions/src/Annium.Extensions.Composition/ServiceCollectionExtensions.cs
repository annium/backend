using Annium.Extensions.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddComposition(this IServiceCollection services)
        {
            services.AddAllTypes()
                .AssignableTo(typeof(Composer<>))
                .Where(x => !x.IsGenericType)
                .AsImplementedInterfaces()
                .InstancePerScope();

            services.AddScoped(typeof(IComposer<>), typeof(CompositionExecutor<>));

            return services;
        }
    }
}