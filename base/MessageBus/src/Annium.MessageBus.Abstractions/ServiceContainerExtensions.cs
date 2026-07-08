using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.MessageBus.Abstractions.Internal;
using Annium.Serialization.Abstractions;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Provides extension methods for registering the transport-agnostic message-bus core.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the shared <see cref="IMessagePublisher"/> and <see cref="IMessageSubscriber"/> implementations
    /// under the default key, and additionally non-keyed (so plain <c>Resolve&lt;IMessagePublisher&gt;()</c> resolves
    /// this default broker). The adapter must register its <see cref="ITransportProducer"/> and
    /// <see cref="ITransportConsumerFactory"/> under the same key (and a default <c>ISerializer&lt;string&gt;</c> must
    /// be available in the container).
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">Configures the core options (e.g. <see cref="MessageBusCoreOptions.SupportsReplay"/>).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddMessageBusCore(
        this IServiceContainer container,
        Action<MessageBusCoreOptions>? configure = null
    ) => container.AddMessageBusCore(MessageBusKeys.Default, isDefault: true, configure);

    /// <summary>
    /// Registers the shared <see cref="IMessagePublisher"/> and <see cref="IMessageSubscriber"/> implementations keyed
    /// by <paramref name="key"/> only (no non-keyed default). The adapter must register its
    /// <see cref="ITransportProducer"/> and <see cref="ITransportConsumerFactory"/> under the same key.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key selecting this broker's stack.</param>
    /// <param name="configure">Configures the core options (e.g. <see cref="MessageBusCoreOptions.SupportsReplay"/>).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddMessageBusCore(
        this IServiceContainer container,
        object key,
        Action<MessageBusCoreOptions>? configure = null
    ) => container.AddMessageBusCore(key, isDefault: false, configure);

    /// <summary>
    /// Registers the shared publisher/subscriber for one broker keyed by <paramref name="key"/>, resolving the transport
    /// SPI (and, for replay, the second subscriber service type) by the same key; when <paramref name="isDefault"/> is
    /// set, also exposes the public services non-keyed by forwarding to the keyed registration.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key selecting this broker's stack.</param>
    /// <param name="isDefault">Whether to additionally expose the public services non-keyed.</param>
    /// <param name="configure">Configures the core options.</param>
    /// <returns>The service container for method chaining.</returns>
    private static IServiceContainer AddMessageBusCore(
        this IServiceContainer container,
        object key,
        bool isDefault,
        Action<MessageBusCoreOptions>? configure
    )
    {
        var options = new MessageBusCoreOptions();
        configure?.Invoke(options);

        // publisher — single service type, keyed directly
        container
            .Add<MessageBusPublisher>(
                (sp, _) =>
                    new MessageBusPublisher(sp.ResolveKeyed<ITransportProducer>(key), sp.Resolve<ISerializer<string>>())
            )
            .AsKeyed<IMessagePublisher>(key)
            .Singleton();

        if (options.SupportsReplay)
        {
            // replay subscriber — one instance under both IMessageSubscriber and IReplayableMessageSubscriber; register
            // the concrete keyed once, then forward both service types to it.
            container
                .Add<ReplayableMessageBusSubscriber>(
                    (sp, _) =>
                        new ReplayableMessageBusSubscriber(
                            sp.ResolveKeyed<ITransportConsumerFactory>(key),
                            sp.ResolveKeyed<ITransportProducer>(key),
                            sp.Resolve<ISerializer<string>>(),
                            sp.Resolve<ILogger>()
                        )
                )
                .AsKeyedSelf(key)
                .Singleton();
            container
                .Add<IMessageSubscriber>((sp, k) => sp.ResolveKeyed<ReplayableMessageBusSubscriber>(k))
                .AsKeyed<IMessageSubscriber>(key)
                .Singleton();
            container
                .Add<IReplayableMessageSubscriber>((sp, k) => sp.ResolveKeyed<ReplayableMessageBusSubscriber>(k))
                .AsKeyed<IReplayableMessageSubscriber>(key)
                .Singleton();
        }
        else
        {
            container
                .Add<MessageBusSubscriber>(
                    (sp, _) =>
                        new MessageBusSubscriber(
                            sp.ResolveKeyed<ITransportConsumerFactory>(key),
                            sp.ResolveKeyed<ITransportProducer>(key),
                            sp.Resolve<ISerializer<string>>(),
                            sp.Resolve<ILogger>()
                        )
                )
                .AsKeyed<IMessageSubscriber>(key)
                .Singleton();
        }

        if (isDefault)
        {
            // expose the public services non-keyed by forwarding to the keyed registration (same singleton)
            container.Add(sp => sp.ResolveKeyed<IMessagePublisher>(key)).As<IMessagePublisher>().Singleton();
            container.Add(sp => sp.ResolveKeyed<IMessageSubscriber>(key)).As<IMessageSubscriber>().Singleton();
            if (options.SupportsReplay)
                container
                    .Add(sp => sp.ResolveKeyed<IReplayableMessageSubscriber>(key))
                    .As<IReplayableMessageSubscriber>()
                    .Singleton();
        }

        return container;
    }
}
