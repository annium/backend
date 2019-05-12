using Annium.Extensions.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScheduler(this IServiceCollection services)
        {
            services.AddSingleton<IIntervalResolver, IntervalResolver>();
            services.AddSingleton<IScheduler, Scheduler>();

            return services;
        }
    }
}