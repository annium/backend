using Annium.Core.DependencyInjection;
using Annium.MessageBus.Abstractions.Internal;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Provides extension methods for registering the transport-agnostic message-bus core.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the shared <see cref="IMessagePublisher"/> and <see cref="IMessageSubscriber"/> implementations
    /// over transport supplied by an adapter. The adapter must additionally register an
    /// <see cref="ITransportProducer"/> and an <see cref="ITransportConsumerFactory"/> (and a default
    /// <c>ISerializer&lt;string&gt;</c> must be available in the container).
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddMessageBusCore(this IServiceContainer container)
    {
        container.Add<IMessagePublisher, MessageBusPublisher>().Singleton();
        container.Add<IMessageSubscriber, MessageBusSubscriber>().Singleton();

        return container;
    }
}
