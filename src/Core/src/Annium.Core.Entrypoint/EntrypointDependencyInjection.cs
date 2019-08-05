using Annium.Core.DependencyInjection;

namespace Annium.Core.Entrypoint
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