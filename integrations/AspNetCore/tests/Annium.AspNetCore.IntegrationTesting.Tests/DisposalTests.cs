using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Completes the disposal contract pinned for <see cref="TestHostBase{TEntryPoint}" />: never-started
/// disposal is already covered by <see cref="LazyLoggerTests.DisposeAsync_HostNotStarted_ThrowsInvalidOperationException" />;
/// this class adds double-dispose and dispose-while-a-request-is-in-flight coverage.
/// </summary>
public class DisposalTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the DisposalTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public DisposalTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that disposing an already-disposed <see cref="TestHostBase{TEntryPoint}" /> a second time does
    /// not throw. This characterizes today's actual behavior — <see cref="TestHostBase{TEntryPoint}.DisposeAsync" />
    /// does not guard against repeated calls, and the underlying <c>WebApplicationFactory{TEntryPoint}.DisposeAsync</c>
    /// it delegates to is itself idempotent — rather than asserting a stronger contract the code does not have.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        // arrange
        var testHost = new TestHost(OutputHelper);
        await testHost.StartAsync();
        await testHost.DisposeAsync();

        // act & assert — a throw here fails the test on its own; no wrapping needed for a "does not throw"
        // expectation.
        await testHost.DisposeAsync();
    }

    /// <summary>
    /// Tests that <see cref="TestHostBase{TEntryPoint}.DisposeAsync" /> completes rather than hanging while a
    /// request is still in flight against the host. Uses <see cref="SlowRequestTestHost" />'s
    /// <see cref="RequestGate" /> to hold the request open deterministically (no <c>Task.Delay</c>) until the
    /// server has genuinely started handling it, then disposes the host concurrently with that pending
    /// request — without ever releasing the gate, the worst case for a hang.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_WhileRequestInFlight_CompletesWithoutHanging()
    {
        // arrange
        var host = new SlowRequestTestHost(OutputHelper);
        var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // fire-and-forget: this request is expected to remain pending for the lifetime of the test, since
        // the gate is deliberately never released.
        _ = client.GetAsync("/slow", TestContext.Current.CancellationToken);

        // wait for a positive signal that the request has genuinely reached the blocking endpoint and is now
        // held open by the gate, rather than assuming a fixed delay is "long enough".
        await host.Gate.Started.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // act — dispose while the request is still pending; the gate is deliberately never released, so if
        // disposal did not tear down the in-flight connection on its own, this would hang instead of
        // completing. Bounded via WaitAsync (consistent with the rest of the suite) purely as a CI safety
        // net against a genuine hang, not as the proof itself — the proof is that this line returns at all.
        await testHost
            .DisposeAsync()
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    }
}
