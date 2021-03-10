using Annium.Infrastructure.WebSockets.Client;
using Annium.Infrastructure.WebSockets.Client.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddWebSocketClient(this IServiceContainer container)
        {
            // public
            container.Add<IClientFactory, ClientFactory>().Singleton();

            // internal
            container.Add<SerializerFactory>().AsSelf().Transient();

            return container;
        }
    }
}