using System;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Infrastructure.MessageBus.Node.Internal;
using Annium.Infrastructure.MessageBus.Node.Internal.Transport;
using Annium.Infrastructure.MessageBus.Node.Transport;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
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

    private static IServiceContainer AddMessageBusBase(this IServiceContainer container)
    {
        container.Add<IMessageBusNode, MessageBusNode>().Singleton();

        container.Add<IMessageBusClient, MessageBusClient>().Singleton();
        container.Add<IMessageBusServer, MessageBusServer>().Singleton();

        return container;
    }
}
