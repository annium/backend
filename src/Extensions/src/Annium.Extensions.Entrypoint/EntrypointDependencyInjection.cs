using Annium.Extensions.DependencyInjection;

namespace Annium.Extensions.Entrypoint
{
    public partial class Entrypoint
    {
        private ServiceProviderBuilder serviceProviderBuilder = new ServiceProviderBuilder();

        public Entrypoint UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
        {
            serviceProviderBuilder.UseServicePack<TServicePack>();

            return this;
        }
    }
}