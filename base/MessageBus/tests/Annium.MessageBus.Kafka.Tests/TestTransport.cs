using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Shared;
using Testcontainers.Kafka;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Conformance-suite seam for the Kafka transport. A single Kafka container is shared across the whole adapter test
/// run (started lazily under a static gate, reaped by the Testcontainers Ryuk sidecar at process exit), and its
/// bootstrap address is fed to <c>AddKafkaMessageBus</c>.
/// </summary>
public sealed class TestTransport : IMessageBusTestTransport
{
    /// <summary>
    /// Serializes lazy container creation across concurrent test-class construction.
    /// </summary>
    private static readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// The shared Kafka container (created once per run).
    /// </summary>
    private static KafkaContainer? _container;

    /// <summary>
    /// The resolved bootstrap servers for this instance's DI configuration.
    /// </summary>
    private string _bootstrapServers = string.Empty;

    /// <summary>
    /// Starts the shared Kafka container on first call (guarded so concurrent test-class construction only starts it
    /// once) and resolves this instance's bootstrap servers from it.
    /// </summary>
    /// <returns>A task that completes once the bootstrap servers are resolved.</returns>
    public async ValueTask StartAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_container is null)
            {
                var container = new KafkaBuilder("confluentinc/cp-kafka:7.6.1").Build();
                await container.StartAsync();
                _container = container;
            }
        }
        finally
        {
            _gate.Release();
        }

        // GetBootstrapAddress may include a scheme (PLAINTEXT://host:port); the configuration normalizes it.
        _bootstrapServers = _container.GetBootstrapAddress();
    }

    /// <summary>
    /// Registers the Kafka message bus into the container, pointed at the started container's bootstrap servers.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    public void Configure(IServiceContainer container) =>
        container.AddKafkaMessageBus(builder => builder.BootstrapServers(_bootstrapServers));

    /// <summary>
    /// Gets the eventual-assertion timeout (ms) suited to the Kafka container's broker latency.
    /// </summary>
    public int DefaultTimeoutMs => 15000;

    /// <summary>
    /// No-op; the shared container is reaped by the Testcontainers Ryuk sidecar at process exit.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
