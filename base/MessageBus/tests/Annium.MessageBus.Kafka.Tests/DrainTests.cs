using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.Kafka.Tests;

/// <summary>
/// Runs the shared graceful-drain conformance suite against the Kafka transport.
/// </summary>
public sealed class DrainTests : DrainConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DrainTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DrainTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
