using System;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Workers.Internal;

namespace Annium.Extensions.Workers;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddWorkers<TData, TWorker>(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TData : IEquatable<TData>
        where TWorker : IWorker<TData>
    {
        container.Add<IWorkerManager<TData>, WorkerManager<TData>>().In(lifetime);
        container.Add<IWorker<TData>, TWorker>().Transient();

        return container;
    }
}