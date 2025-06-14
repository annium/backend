using System;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.MessageBus.Node.Internal;
using Annium.Infrastructure.MessageBus.Node.Internal.Transport;

// ReSharper disable InconsistentNaming

namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Provides extension methods for configuring message bus services in the service container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds NetMQ-based message bus services to the service container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">A configuration action for setting up network endpoints and options.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddNetMQMessageBus(
        this IServiceContainer container,
        Action<IServiceProvider, INetworkConfigurationBuilder> configure
    )
    {
        container
            .Add(sp =>
            {
                var builder = sp.Resolve<NetworkConfigurationBuilder>();
                configure(sp, builder);
                return builder.Build();
            })
            .AsSelf()
            .AsInterfaces()
            .Singleton();
        container.Add<IMessageBusSocket, NetMQMessageBusSocket>().Singleton();
        container.Add<NetworkConfigurationBuilder>().AsSelf().Singleton();

        return container.AddMessageBusBase();
    }

    /// <summary>
    /// Adds in-memory message bus services to the service container for testing and development scenarios.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">A configuration action for setting up in-memory message bus options.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddInMemoryMessageBus(
        this IServiceContainer container,
        Action<IServiceProvider, IInMemoryConfigurationBuilder> configure
    )
    {
        container
            .Add(sp =>
            {
                var builder = sp.Resolve<InMemoryConfigurationBuilder>();
                configure(sp, builder);
                return builder.Build();
            })
            .AsSelf()
            .AsInterfaces()
            .Singleton();
        container.Add<IMessageBusSocket, InMemoryMessageBusSocket>().Singleton();
        container.Add<InMemoryConfigurationBuilder>().AsSelf().Singleton();

        return container.AddMessageBusBase();
    }

    /// <summary>
    /// Adds the core message bus services that are common to all transport implementations.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <returns>The service container for method chaining.</returns>
    private static IServiceContainer AddMessageBusBase(this IServiceContainer container)
    {
        container.Add<IMessageBusNode, MessageBusNode>().Singleton();

        container.Add<IMessageBusClient, MessageBusClient>().Singleton();
        container.Add<IMessageBusServer, MessageBusServer>().Singleton();

        return container;
    }
}
