using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Annium.Testing;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Tests for request-response messaging functionality using InMemory transport.
/// </summary>
public class RequestTests : RequestTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RequestTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests echo request-response messaging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Echo()
    {
        this.Trace("start");

        await Echo_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that a non-Ok handler status surfaces to the caller.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Fail()
    {
        this.Trace("start");

        await Fail_ReturnsNonOkStatus_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that caller-side cancellation yields an Aborted status.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Cancel()
    {
        this.Trace("start");

        await Cancel_ReturnsAborted_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that a throwing handler does not fault the connection.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Throw_KeepsConnectionAlive()
    {
        this.Trace("start");

        await Throw_KeepsConnectionAlive_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that double-dispose of the client is idempotent.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Dispose_Twice_DoesNotThrow()
    {
        this.Trace("start");

        await Dispose_Twice_DoesNotThrow_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that an explicit disconnect raises OnDisconnected with a local-close status.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Disconnect_FiresOnDisconnected()
    {
        this.Trace("start");

        await Disconnect_FiresOnDisconnected_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies the InMemory channel is disconnected idempotently: an explicit disconnect followed by
    /// disposal (a second disconnect) raises OnDisconnected exactly once, not twice.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Disconnect_FiresOnDisconnectedOnce()
    {
        this.Trace("start");

        // arrange
        await using var client = await GetClient();
        var count = 0;
        var firstFired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        client.OnDisconnected += _ =>
        {
            Interlocked.Increment(ref count);
            firstFired.TrySetResult();
        };

        // act — explicit disconnect, then dispose (which disconnects again)
        client.Disconnect();
        await firstFired.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await client.DisposeAsync();

        // assert — the second disconnect must be a no-op
        Volatile.Read(ref count).Is(1);

        this.Trace("done");
    }

    /// <summary>
    /// Tests that a no-response-data request (SendAsync) returns Ok.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Send()
    {
        this.Trace("start");

        await Send_ReturnsOkStatus_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that the FetchAsync default-value overload returns actual data on success.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task FetchWithDefault_Success()
    {
        this.Trace("start");

        await FetchWithDefault_Success_ReturnsData_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that the FetchAsync default-value overload returns the default on a null response.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task FetchWithDefault_Cancelled()
    {
        this.Trace("start");

        await FetchWithDefault_Cancelled_ReturnsDefault_Base();

        this.Trace("done");
    }
}
