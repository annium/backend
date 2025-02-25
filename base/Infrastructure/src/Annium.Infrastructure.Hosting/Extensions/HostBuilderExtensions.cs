using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseServicePack<TServicePack>(this IHostBuilder builder)
        where TServicePack : ServicePackBase, new() =>
        builder.UseServiceProviderFactory(new ServiceProviderFactory(b => b.UseServicePack<TServicePack>()));
}
