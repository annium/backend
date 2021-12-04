using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Entrypoint;

public partial class Entrypoint
{
    private readonly IServiceProviderBuilder _serviceProviderBuilder =
        new ServiceProviderFactory().CreateBuilder(new ServiceCollection());

    public Entrypoint UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        _serviceProviderBuilder.UseServicePack<TServicePack>();

        return this;
    }
}