using Annium.Core.DependencyInjection;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.InMemory.Internal;

namespace Annium.MessageBus.InMemory;

/// <summary>
/// Provides extension methods for registering the in-memory message-bus adapter.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the in-memory transport (a single in-process broker) and the shared message-bus core. Requires a
    /// default <c>ISerializer&lt;string&gt;</c> to be available in the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddInMemoryMessageBus(this IServiceContainer container)
    {
        // single broker instance exposed as both producer and consumer-factory
        container.Add<InMemoryTransport>().As<ITransportProducer>().As<ITransportConsumerFactory>().Singleton();

        return container.AddMessageBusCore();
    }
}
