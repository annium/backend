using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Runs the shared delivery-mode conformance suite against the Kafka transport (at-least-once redelivery via
/// seek-on-abandon; at-most-once drop via commit-before-process).
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
