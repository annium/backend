using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// Runs the shared wildcard conformance suite against the RabbitMQ transport. Unlike Kafka, RabbitMQ topic bindings
/// match immediately (no metadata-discovery delay) and unrouted messages are not retained, so the shared fixed-subject
/// bodies run as-is on the shared broker.
/// </summary>
public sealed class WildcardTests : WildcardConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WildcardTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public WildcardTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
