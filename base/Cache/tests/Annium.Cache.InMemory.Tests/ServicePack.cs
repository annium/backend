using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.InMemory.Tests;

/// <summary>
/// Service pack for configuring in-memory cache dependencies for testing.
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers the in-memory cache services required for testing.
    /// </summary>
    /// <param name="container">The service container to register services with.</param>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddInMemoryCache(ServiceLifetime.Singleton);
        return Task.CompletedTask;
    }
}
