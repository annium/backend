using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddHostedService<THostedService>(this IServiceContainer container)
        where THostedService : class, IHostedService
    {
        container.Collection.AddHostedService<THostedService>();

        return container;
    }
}
