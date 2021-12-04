using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection;

public interface IServiceProviderBuilder
{
    IServiceProviderBuilder UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new();

    IServiceProviderBuilder UseServicePack(ServicePackBase servicePack);

    ServiceProvider Build();
}