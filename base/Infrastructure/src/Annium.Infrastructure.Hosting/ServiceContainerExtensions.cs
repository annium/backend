using Annium.Core.DependencyInjection.Container;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Annium.Infrastructure.Hosting;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddHostedService<THostedService>(this IServiceContainer container)
        where THostedService : class, IHostedService
    {
        container.Collection.AddHostedService<THostedService>();

        return container;
    }
}
