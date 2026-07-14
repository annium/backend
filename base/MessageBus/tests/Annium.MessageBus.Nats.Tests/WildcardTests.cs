using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// Runs the shared wildcard conformance suite against the NATS transport (native <c>*</c>/<c>&gt;</c> matching).
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
