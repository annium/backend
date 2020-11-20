using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.New
{
    public interface IServiceProviderBuilder
    {
        IServiceProviderBuilder UseServicePack<TServicePack>()
            where TServicePack : ServicePackBase, new();

        ServiceProvider Build();
    }
}