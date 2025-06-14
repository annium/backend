using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Annium.Infrastructure.Hosting;

/// <summary>
/// Provides extension methods for registering hosted services with the service container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers a hosted service with the service container.
    /// </summary>
    /// <typeparam name="THostedService">The type of hosted service to register, which must implement IHostedService.</typeparam>
    /// <param name="container">The service container to register the hosted service with.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddHostedService<THostedService>(this IServiceContainer container)
        where THostedService : class, IHostedService
    {
        container.Collection.AddHostedService<THostedService>();

        return container;
    }
}
