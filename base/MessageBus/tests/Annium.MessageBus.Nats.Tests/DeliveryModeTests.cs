using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// Runs the shared delivery-mode conformance suite against the NATS transport (at-least-once redelivery via JetStream
/// nak; at-most-once drop via Core NATS with no acknowledgement).
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
