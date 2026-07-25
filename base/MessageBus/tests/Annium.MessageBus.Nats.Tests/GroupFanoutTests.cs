using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// Runs the shared consumer-group conformance suite against the NATS transport (competing = shared JetStream durable;
/// fan-out = per-subscription ephemeral consumer).
/// </summary>
public sealed class GroupFanoutTests : GroupFanoutConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupFanoutTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public GroupFanoutTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
