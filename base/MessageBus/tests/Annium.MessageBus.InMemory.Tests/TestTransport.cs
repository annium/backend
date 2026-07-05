using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Shared;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Conformance-suite seam for the in-memory transport: no broker lifecycle, just DI registration.
/// </summary>
public sealed class TestTransport : IMessageBusTestTransport
{
    /// <inheritdoc />
    public void Configure(IServiceContainer container) => container.AddInMemoryMessageBus();

    /// <inheritdoc />
    public ValueTask StartAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
