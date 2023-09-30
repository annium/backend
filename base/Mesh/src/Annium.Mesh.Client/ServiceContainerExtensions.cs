using Annium.Infrastructure.WebSockets.Client;
using Annium.Infrastructure.WebSockets.Client.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddWebSocketClient(this IServiceContainer container)
    {
        // public
        container.Add<IClientFactory, ClientFactory>().Singleton();
        container.Add<ITestClientFactory, TestClientFactory>().Singleton();

        // internal
        container.Add<Serializer>().AsSelf().Transient();

        return container;
    }
}