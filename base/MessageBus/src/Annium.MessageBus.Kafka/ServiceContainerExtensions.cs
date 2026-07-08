using System;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.Kafka.Internal;

namespace Annium.MessageBus.Kafka;

/// <summary>
/// Provides extension methods for registering the Kafka message-bus adapter.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the Kafka transport (a shared producer plus a consumer factory) and the shared message-bus core with
    /// replay support. Requires a default <c>ISerializer&lt;string&gt;</c> to be available in the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    /// <param name="configure">Configures the Kafka connection (bootstrap servers).</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddKafkaMessageBus(
        this IServiceContainer container,
        Action<IKafkaConfigurationBuilder> configure
    )
    {
        container
            .Add(_ =>
            {
                var builder = new KafkaConfigurationBuilder();
                configure(builder);
                return builder.Build();
            })
            .AsSelf()
            .Singleton();

        // admin (topic ensure + partition lookup) — DI-managed singleton, resolved lazily by consumers, disposed by DI
        container.Add<KafkaAdmin>().As<IKafkaAdmin>().Singleton();

        // single transport instance exposed as both producer and consumer-factory
        container.Add<KafkaTransport>().As<ITransportProducer>().As<ITransportConsumerFactory>().Singleton();

        return container.AddMessageBusCore(options => options.SupportsReplay = true);
    }
}
