using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Entrypoint
{
    public partial class Entrypoint
    {
        private readonly IServiceProviderBuilder serviceProviderBuilder =
            new ServiceProviderFactory().CreateBuilder(new ServiceCollection());

        public Entrypoint UseServicePack<TServicePack>()
            where TServicePack : ServicePackBase, new()
        {
            serviceProviderBuilder.UseServicePack<TServicePack>();

            return this;
        }
    }
}