using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
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
    /// the shared message-bus core under the default key, also exposed non-keyed. RabbitMQ does not support replay, so
    /// the core is registered without it. Requires a default <c>ISerializer&lt;string&gt;</c> in the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">Configures the RabbitMQ connection (URI, exchange).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddRabbitMqMessageBus(
        this IServiceContainer container,
        Action<IRabbitMqConfigurationBuilder> configure
    ) => container.AddRabbitMqMessageBus(MessageBusKeys.Default, configure, isDefault: true);

    /// <summary>
    /// Registers the RabbitMQ transport and the shared message-bus core keyed by <paramref name="key"/> only, so several
    /// RabbitMQ connections/exchanges can coexist in one container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key selecting this broker's stack.</param>
    /// <param name="configure">Configures the RabbitMQ connection (URI, exchange).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddRabbitMqMessageBus(
        this IServiceContainer container,
        object key,
        Action<IRabbitMqConfigurationBuilder> configure
    ) => container.AddRabbitMqMessageBus(key, configure, isDefault: false);

    /// <summary>
    /// Registers the keyed RabbitMQ configuration, shared connection, and transport (one instance under both transport
    /// SPI service types), then the plain core. When <paramref name="isDefault"/> is set, the configuration is also
    /// exposed non-keyed.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key.</param>
    /// <param name="configure">Configures the RabbitMQ connection.</param>
    /// <param name="isDefault">Whether the default (non-keyed) surface is exposed.</param>
    /// <returns>The service container for method chaining.</returns>
    private static IServiceContainer AddRabbitMqMessageBus(
        this IServiceContainer container,
        object key,
        Action<IRabbitMqConfigurationBuilder> configure,
        bool isDefault
    )
    {
        container
            .Add<RabbitMqConfiguration>(
                (_, _) =>
                {
                    var builder = new RabbitMqConfigurationBuilder();
                    configure(builder);
                    return builder.Build();
                }
            )
            .AsKeyed<RabbitMqConfiguration>(key)
            .Singleton();

        // shared connection (channel factory + exchange) — keyed, DI-managed singleton, disposed by DI after the transport
        container
            .Add<RabbitMqConnection>(
                (sp, _) => new RabbitMqConnection(sp.ResolveKeyed<RabbitMqConfiguration>(key), sp.Resolve<ILogger>())
            )
            .AsKeyedSelf(key)
            .Singleton();

        // single transport instance (keyed), exposed as both producer and consumer-factory via keyed forwarders
        container
            .Add<RabbitMqTransport>(
                (sp, _) => new RabbitMqTransport(sp.ResolveKeyed<RabbitMqConnection>(key), sp.Resolve<ILogger>())
            )
            .AsKeyedSelf(key)
            .Singleton();
        container
            .Add<ITransportProducer>((sp, k) => sp.ResolveKeyed<RabbitMqTransport>(k))
            .AsKeyed<ITransportProducer>(key)
            .Singleton();
        container
            .Add<ITransportConsumerFactory>((sp, k) => sp.ResolveKeyed<RabbitMqTransport>(k))
            .AsKeyed<ITransportConsumerFactory>(key)
            .Singleton();

        if (isDefault)
            container.Add(sp => sp.ResolveKeyed<RabbitMqConfiguration>(key)).As<RabbitMqConfiguration>().Singleton();

        return isDefault ? container.AddMessageBusCore() : container.AddMessageBusCore(key);
    }
}
