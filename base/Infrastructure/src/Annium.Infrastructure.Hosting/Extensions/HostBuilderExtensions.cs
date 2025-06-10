using Annium.Core.DependencyInjection.Packs;
using Annium.Core.DependencyInjection.Plugins;
using Microsoft.Extensions.Hosting;

namespace Annium.Infrastructure.Hosting.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseServicePack<TServicePack>(this IHostBuilder builder)
        where TServicePack : ServicePackBase, new() =>
        builder.UseServiceProviderFactory(new ServiceProviderFactory(b => b.UseServicePack<TServicePack>()));
}
