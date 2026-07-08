using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.InMemory.Internal;

namespace Annium.MessageBus.InMemory;

/// <summary>
/// Provides extension methods for registering the in-memory message-bus adapter.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the in-memory transport (a single in-process broker) and the shared message-bus core under the default
    /// key (also exposed non-keyed). Requires a default <c>ISerializer&lt;string&gt;</c> to be available in the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddInMemoryMessageBus(this IServiceContainer container) =>
        container.AddInMemoryMessageBus(MessageBusKeys.Default, isDefault: true);

    /// <summary>
    /// Registers the in-memory transport and the shared message-bus core keyed by <paramref name="key"/> only, so several
    /// isolated in-memory brokers can coexist in one container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key selecting this broker's stack.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddInMemoryMessageBus(this IServiceContainer container, object key) =>
        container.AddInMemoryMessageBus(key, isDefault: false);

    /// <summary>
    /// Registers the in-memory transport (one instance under both transport SPI service types) and the core, all keyed
    /// by <paramref name="key"/>.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="key">The registration key.</param>
    /// <param name="isDefault">Whether the core is additionally exposed non-keyed.</param>
    /// <returns>The service container for method chaining.</returns>
    private static IServiceContainer AddInMemoryMessageBus(this IServiceContainer container, object key, bool isDefault)
    {
        // single broker instance, keyed, exposed as both producer and consumer-factory via keyed forwarders
        container
            .Add<InMemoryTransport>((sp, _) => new InMemoryTransport(sp.Resolve<ILogger>()))
            .AsKeyedSelf(key)
            .Singleton();
        container
            .Add<ITransportProducer>((sp, k) => sp.ResolveKeyed<InMemoryTransport>(k))
            .AsKeyed<ITransportProducer>(key)
            .Singleton();
        container
            .Add<ITransportConsumerFactory>((sp, k) => sp.ResolveKeyed<InMemoryTransport>(k))
            .AsKeyed<ITransportConsumerFactory>(key)
            .Singleton();

        return isDefault ? container.AddMessageBusCore() : container.AddMessageBusCore(key);
    }
}
