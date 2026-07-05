using Annium.MessageBus.Tests.Shared;
using Xunit;

namespace Annium.MessageBus.InMemory.Tests;

/// <summary>
/// Runs the shared retry/dead-letter conformance suite against the in-memory transport.
/// </summary>
public sealed class RetryDlqTests : RetryDlqConformanceTests<TestTransport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryDlqTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RetryDlqTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
