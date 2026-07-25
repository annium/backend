using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// Runs the shared publish/subscribe conformance suite against the NATS transport (JetStream default delivery).
/// </summary>
public sealed class PubSubTests : PubSubConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PubSubTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public PubSubTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
