using System;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.RabbitMq.Internal;

namespace Annium.MessageBus.RabbitMq;

/// <summary>
/// Provides extension methods for registering the RabbitMQ message-bus adapter.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the RabbitMQ transport (a shared connection, a publisher-confirms channel, and a consumer factory) and
    /// the shared message-bus core. RabbitMQ does not support replay, so the core is registered without it. Requires a
    /// default <c>ISerializer&lt;string&gt;</c> to be available in the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">Configures the RabbitMQ connection (URI, exchange).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddRabbitMqMessageBus(
        this IServiceContainer container,
        Action<IRabbitMqConfigurationBuilder> configure
    )
    {
        container
            .Add(_ =>
            {
                var builder = new RabbitMqConfigurationBuilder();
                configure(builder);
                return builder.Build();
            })
            .AsSelf()
            .Singleton();

        // shared connection (channel factory + exchange) — DI-managed singleton, disposed by DI after the transport
        container.Add<RabbitMqConnection>().AsSelf().Singleton();

        // single transport instance exposed as both producer and consumer-factory
        container.Add<RabbitMqTransport>().As<ITransportProducer>().As<ITransportConsumerFactory>().Singleton();

        return container.AddMessageBusCore();
    }
}
