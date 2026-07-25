using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Load.Shared;
using Testcontainers.RabbitMq;

namespace Annium.MessageBus.RabbitMq.Load;

/// <summary>
/// The load-harness transport seam for RabbitMQ: starts a RabbitMQ container and registers the RabbitMQ message-bus
/// adapter. Publish concurrency is capped at 1 — the adapter shares a single publish channel, which is not safe under
/// concurrent publishing.
/// </summary>
public sealed class RabbitMqLoadTransport : ILoadTransport
{
    /// <summary>
    /// The RabbitMQ container.
    /// </summary>
    private RabbitMqContainer? _container;

    /// <summary>
    /// The resolved connection string.
    /// </summary>
    private string _connectionString = string.Empty;

    /// <summary>
    /// Gets the broker display name.
    /// </summary>
    public string BrokerName => "RabbitMq";

    /// <summary>
    /// Gets the publish-concurrency cap (1 — the adapter's single publish channel is not concurrency-safe).
    /// </summary>
    public int MaxPublisherConcurrency => 1;

    /// <summary>
    /// Starts the RabbitMQ container and captures its connection string.
    /// </summary>
    /// <returns>A task that completes once the broker is ready.</returns>
    public async Task StartAsync()
    {
        var container = new RabbitMqBuilder("rabbitmq:3.13").Build();
        await container.StartAsync();
        _container = container;
        _connectionString = container.GetConnectionString();
    }

    /// <summary>
    /// Registers the RabbitMQ message-bus adapter into the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    public void Configure(IServiceContainer container) =>
        container.AddRabbitMqMessageBus(builder => builder.ConnectionUri(_connectionString));

    /// <summary>
    /// Disposes the RabbitMQ container.
    /// </summary>
    /// <returns>A task that completes once the container has been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
