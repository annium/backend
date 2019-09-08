using Annium.Core.Application.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCore(this IServiceCollection services)
        {
            services.AddSingleton(TypeManager.Instance);
        }
    }
}