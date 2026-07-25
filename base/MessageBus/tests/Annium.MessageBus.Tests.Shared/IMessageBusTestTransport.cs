using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.MessageBus.Tests.Shared;

/// <summary>
/// The seam a transport adapter provides so the shared conformance suite can run against it: how to register the
/// transport in DI and how to bring its broker up. In-memory adapters are no-op on lifecycle; real adapters start a
/// container (e.g. shared static Testcontainers instance) in <see cref="StartAsync"/> and expose its connection to
/// <see cref="Configure"/>.
/// </summary>
public interface IMessageBusTestTransport : IAsyncDisposable
{
    /// <summary>
    /// Registers the transport (and the message-bus core) into the container. Called during DI build, after
    /// <see cref="StartAsync"/>, so a real adapter can read its started broker's connection here.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    void Configure(IServiceContainer container);

    /// <summary>
    /// Brings the transport's broker up (no-op for in-memory). Invoked before the DI container is built.
    /// </summary>
    /// <returns>A task that completes once the broker is ready.</returns>
    ValueTask StartAsync();

    /// <summary>
    /// Gets the eventual-assertion timeout (ms) suited to this transport's latency. Real brokers override with a
    /// larger value.
    /// </summary>
    int DefaultTimeoutMs => 3000;
}
