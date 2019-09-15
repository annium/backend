using Annium.Core.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddReflectionTools(this IServiceCollection services)
        {
            services.AddSingleton(TypeManager.Instance);
        }
    }
}