using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Load.Shared;
using Testcontainers.Kafka;

namespace Annium.MessageBus.Kafka.Load;

/// <summary>
/// The load-harness transport seam for Kafka: starts a Kafka container and registers the Kafka message-bus adapter.
/// </summary>
public sealed class KafkaLoadTransport : ILoadTransport
{
    /// <summary>
    /// The Kafka container.
    /// </summary>
    private KafkaContainer? _container;

    /// <summary>
    /// The resolved bootstrap servers.
    /// </summary>
    private string _bootstrapServers = string.Empty;

    /// <summary>
    /// Gets the broker display name.
    /// </summary>
    public string BrokerName => "Kafka";

    /// <summary>
    /// Starts the Kafka container and captures its bootstrap address.
    /// </summary>
    /// <returns>A task that completes once the broker is ready.</returns>
    public async Task StartAsync()
    {
        var container = new KafkaBuilder("confluentinc/cp-kafka:7.6.1").Build();
        await container.StartAsync();
        _container = container;
        _bootstrapServers = container.GetBootstrapAddress();
    }

    /// <summary>
    /// Registers the Kafka message-bus adapter into the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    public void Configure(IServiceContainer container) =>
        container.AddKafkaMessageBus(builder => builder.BootstrapServers(_bootstrapServers));

    /// <summary>
    /// Disposes the Kafka container.
    /// </summary>
    /// <returns>A task that completes once the container has been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
