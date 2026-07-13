using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.MessageBus.Tests.Load.Shared;

/// <summary>
/// The seam a broker adapter provides so the shared load harness can run against it: how to name it, how to bring its
/// broker up (plus any broker-specific provisioning, e.g. a NATS JetStream stream), and how to register the transport
/// into the DI container.
/// </summary>
public interface ILoadTransport : IAsyncDisposable
{
    /// <summary>
    /// Gets the broker display name used in the printed report (e.g. "Kafka").
    /// </summary>
    string BrokerName { get; }

    /// <summary>
    /// Gets the maximum number of publish calls the harness may issue concurrently against this transport. Adapters
    /// whose producer is not thread-safe under concurrent publishing (RabbitMQ shares one publish channel) return 1;
    /// others return 0 (meaning "no per-transport cap — use the scenario's publisher count").
    /// </summary>
    int MaxPublisherConcurrency => 0;

    /// <summary>
    /// Brings the broker container up and performs any broker-specific provisioning needed before the DI container is
    /// built (e.g. creating the JetStream stream for NATS).
    /// </summary>
    /// <returns>A task that completes once the broker is ready.</returns>
    Task StartAsync();

    /// <summary>
    /// Registers the transport (and the message-bus core) into the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    void Configure(IServiceContainer container);
}
