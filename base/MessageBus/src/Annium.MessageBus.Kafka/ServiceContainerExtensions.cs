using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Kafka.Internal;

namespace Annium.MessageBus.Kafka;

/// <summary>
/// Provides extension methods for registering the Kafka message-bus adapter.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the Kafka transport and the shared message-bus core (with replay support) under the default key, also
    /// exposed non-keyed. Requires a default <c>ISerializer&lt;string&gt;</c> to be available in the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">Configures the Kafka connection (bootstrap servers).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddKafkaMessageBus(
        this IServiceContainer container,
        Action<IKafkaConfigurationBuilder> configure
    ) => container.AddKafkaMessageBus(MessageBusKeys.Default, configure, isDefault: true);

    /// <summary>
    /// Registers the Kafka transport and the shared message-bus core (with replay support) keyed by <paramref name="key"/>
    /// only, so several Kafka clusters can coexist in one container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key selecting this broker's stack.</param>
    /// <param name="configure">Configures the Kafka connection (bootstrap servers).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddKafkaMessageBus(
        this IServiceContainer container,
        object key,
        Action<IKafkaConfigurationBuilder> configure
    ) => container.AddKafkaMessageBus(key, configure, isDefault: false);

    /// <summary>
    /// Registers the keyed Kafka configuration, admin, and transport (one instance under both transport SPI service
    /// types), then the replay-capable core. When <paramref name="isDefault"/> is set, the configuration is also exposed
    /// non-keyed (so the default broker's <see cref="KafkaConfiguration"/> resolves by type).
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key.</param>
    /// <param name="configure">Configures the Kafka connection.</param>
    /// <param name="isDefault">Whether the default (non-keyed) surface is exposed.</param>
    /// <returns>The service container for method chaining.</returns>
    private static IServiceContainer AddKafkaMessageBus(
        this IServiceContainer container,
        object key,
        Action<IKafkaConfigurationBuilder> configure,
        bool isDefault
    )
    {
        container
            .Add<KafkaConfiguration>(
                (_, _) =>
                {
                    var builder = new KafkaConfigurationBuilder();
                    configure(builder);
                    return builder.Build();
                }
            )
            .AsKeyed<KafkaConfiguration>(key)
            .Singleton();

        // admin (topic ensure + partition lookup) — keyed, DI-managed singleton, disposed by DI
        container
            .Add<KafkaAdmin>((sp, _) => new KafkaAdmin(sp.ResolveKeyed<KafkaConfiguration>(key)))
            .AsKeyed<IKafkaAdmin>(key)
            .Singleton();

        // single transport instance (keyed), exposed as both producer and consumer-factory via keyed forwarders
        container
            .Add<KafkaTransport>(
                (sp, _) =>
                    new KafkaTransport(
                        sp.ResolveKeyed<KafkaConfiguration>(key),
                        sp.ResolveKeyed<IKafkaAdmin>(key),
                        sp.Resolve<ILogger>()
                    )
            )
            .AsKeyedSelf(key)
            .Singleton();
        container
            .Add<ITransportProducer>((sp, k) => sp.ResolveKeyed<KafkaTransport>(k))
            .AsKeyed<ITransportProducer>(key)
            .Singleton();
        container
            .Add<ITransportConsumerFactory>((sp, k) => sp.ResolveKeyed<KafkaTransport>(k))
            .AsKeyed<ITransportConsumerFactory>(key)
            .Singleton();

        if (isDefault)
            container.Add(sp => sp.ResolveKeyed<KafkaConfiguration>(key)).As<KafkaConfiguration>().Singleton();

        return isDefault
            ? container.AddMessageBusCore(options => options.SupportsReplay = true)
            : container.AddMessageBusCore(key, options => options.SupportsReplay = true);
    }
}
