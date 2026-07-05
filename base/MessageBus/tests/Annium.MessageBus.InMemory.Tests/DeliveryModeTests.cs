using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Runs the shared delivery-mode conformance suite against the in-memory transport.
/// </summary>
public sealed class DeliveryModeTests : DeliveryModeConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryModeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DeliveryModeTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
