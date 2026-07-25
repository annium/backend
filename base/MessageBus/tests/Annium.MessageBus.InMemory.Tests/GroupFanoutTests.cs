using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Runs the shared group/fan-out conformance suite against the in-memory transport.
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
