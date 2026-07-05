using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Runs the shared drain conformance suite against the in-memory transport.
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
