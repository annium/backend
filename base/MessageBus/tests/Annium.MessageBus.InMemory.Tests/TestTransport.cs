using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Shared;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Conformance-suite seam for the in-memory transport: no broker lifecycle, just DI registration.
/// </summary>
public sealed class TestTransport : IMessageBusTestTransport
{
    /// <summary>
    /// Registers the in-memory message bus into the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    public void Configure(IServiceContainer container) => container.AddInMemoryMessageBus();

    /// <summary>
    /// No-op; the in-memory transport has no broker lifecycle to start.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask StartAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// No-op; the in-memory transport has no broker resources to release.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
