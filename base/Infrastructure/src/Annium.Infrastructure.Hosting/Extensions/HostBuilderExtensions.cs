using Annium.Core.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Annium.Infrastructure.Hosting;

/// <summary>
/// Provides extension methods for configuring IHostBuilder with service packs.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures the host builder to use a specific service pack for dependency injection.
    /// </summary>
    /// <typeparam name="TServicePack">The type of service pack to use, which must inherit from ServicePackBase and have a parameterless constructor.</typeparam>
    /// <param name="builder">The host builder to configure.</param>
    /// <returns>The configured host builder.</returns>
    public static IHostBuilder UseServicePack<TServicePack>(this IHostBuilder builder)
        where TServicePack : ServicePackBase, new() =>
        builder.UseServiceProviderFactory(new ServiceProviderFactory(b => b.UseServicePack<TServicePack>()));
}
