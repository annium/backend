using Annium.Extensions.Jobs;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddScheduler(this IServiceContainer container)
        {
            container.Add<IIntervalResolver, IntervalResolver>().Singleton();
            container.Add<IScheduler, Scheduler>().Singleton();

            return container;
        }
    }
}