using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Redis.Tests;

/// <summary>
/// Service pack for configuring Redis test dependencies and database setup
/// </summary>
internal class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers Redis services and test database configuration
    /// </summary>
    /// <param name="container">Service container for dependency registration</param>
    /// <param name="provider">Service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddRedis();
        container.Add<Database>().AsSelf().Singleton();
        container.Add(sp => sp.Resolve<Database>().Config).AsSelf().Singleton();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets up the test environment by initializing the Redis database container
    /// </summary>
    /// <param name="provider">Service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    public override async Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        await provider.Resolve<Database>().InitAsync();
    }
}
