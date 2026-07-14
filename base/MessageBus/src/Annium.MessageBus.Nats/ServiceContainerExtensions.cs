using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Nats.Internal;

namespace Annium.MessageBus.Nats;

/// <summary>
/// Provides extension methods for registering the NATS message-bus adapter.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the NATS transport and the shared message-bus core (with replay support) under the default key, also
    /// exposed non-keyed. Requires a default <c>ISerializer&lt;string&gt;</c> to be available in the container, and a
    /// JetStream stream capturing the used subjects to be provisioned externally.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">Configures the NATS connection (server URL).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddNatsMessageBus(
        this IServiceContainer container,
        Action<INatsConfigurationBuilder> configure
    ) => container.AddNatsMessageBus(MessageBusKeys.Default, configure, isDefault: true);

    /// <summary>
    /// Registers the NATS transport and the shared message-bus core (with replay support) keyed by <paramref name="key"/>
    /// only, so several NATS servers can coexist in one container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key selecting this broker's stack.</param>
    /// <param name="configure">Configures the NATS connection (server URL).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddNatsMessageBus(
        this IServiceContainer container,
        object key,
        Action<INatsConfigurationBuilder> configure
    ) => container.AddNatsMessageBus(key, configure, isDefault: false);

    /// <summary>
    /// Registers the keyed NATS configuration, shared connection, and transport (one instance under both transport SPI
    /// service types), then the replay-capable core. When <paramref name="isDefault"/> is set, the configuration is also
    /// exposed non-keyed (so the default broker's <see cref="NatsConfiguration"/> resolves by type).
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key.</param>
    /// <param name="configure">Configures the NATS connection.</param>
    /// <param name="isDefault">Whether the default (non-keyed) surface is exposed.</param>
    /// <returns>The service container for method chaining.</returns>
    private static IServiceContainer AddNatsMessageBus(
        this IServiceContainer container,
        object key,
        Action<INatsConfigurationBuilder> configure,
        bool isDefault
    )
    {
        container
            .Add<NatsConfiguration>(
                (_, _) =>
                {
                    var builder = new NatsConfigurationBuilder();
                    configure(builder);
                    return builder.Build();
                }
            )
            .AsKeyed<NatsConfiguration>(key)
            .Singleton();

        // shared connection (Core + JetStream) — keyed, DI-managed singleton, disposed by DI
        container
            .Add<NatsConnectionHolder>(
                (sp, _) => new NatsConnectionHolder(sp.ResolveKeyed<NatsConfiguration>(key), sp.Resolve<ILogger>())
            )
            .AsKeyedSelf(key)
            .Singleton();

        // single transport instance (keyed), exposed as both producer and consumer-factory via keyed forwarders
        container
            .Add<NatsTransport>(
                (sp, _) => new NatsTransport(sp.ResolveKeyed<NatsConnectionHolder>(key), sp.Resolve<ILogger>())
            )
            .AsKeyedSelf(key)
            .Singleton();
        container
            .Add<ITransportProducer>((sp, k) => sp.ResolveKeyed<NatsTransport>(k))
            .AsKeyed<ITransportProducer>(key)
            .Singleton();
        container
            .Add<ITransportConsumerFactory>((sp, k) => sp.ResolveKeyed<NatsTransport>(k))
            .AsKeyed<ITransportConsumerFactory>(key)
            .Singleton();

        if (isDefault)
            container.Add(sp => sp.ResolveKeyed<NatsConfiguration>(key)).As<NatsConfiguration>().Singleton();

        return isDefault
            ? container.AddMessageBusCore(options => options.SupportsReplay = true)
            : container.AddMessageBusCore(key, options => options.SupportsReplay = true);
    }
}
