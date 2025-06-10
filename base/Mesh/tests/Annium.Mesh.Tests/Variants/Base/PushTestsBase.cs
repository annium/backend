using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Base;

/// <summary>
/// Base class for push-based mesh tests, providing common functionality for testing server push scenarios.
/// </summary>
/// <typeparam name="TBehavior">The behavior type that defines server configuration and running logic.</typeparam>
public abstract class PushTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PushTestsBase{TBehavior}"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    protected PushTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Base test method for counter push functionality, verifying that counter values are received in order.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Counter_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("get client");
        var log = new ConcurrentQueue<int>();
        await using var client = await GetClient();

        // act
        this.Trace("listen for while");
        using (var _ = client.Demo.ListenCounter().Subscribe(x => log.Enqueue(x.Value)))
        {
            this.Trace("wait for log entries");
            await Expect.ToAsync(() => log.Count.IsGreaterOrEqual(5));
        }

        // assert
        this.Trace("get log snapshot");
        var logData = log.ToArray();

        this.Trace("ensure snapshot entries are ordered");
        logData.OrderBy(x => x).ToArray().SequenceEqual(logData).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Base test method for concurrent counter push scenarios, running multiple counter listeners simultaneously.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Counter_Bundle_Base()
    {
        this.Trace("start");

        // arrange
        var range = Enumerable.Range(0, 10).ToArray();
        await Task.WhenAll(
            range.Select(async _ =>
            {
                var sample = new PushSample<TBehavior>(Get<ITestOutputHelper>());
                try
                {
                    await sample.InitializeAsync();
                    await sample.RunAsync();
                }
                finally
                {
                    await sample.DisposeAsync();
                }
            })
        );

        this.Trace("done");
    }
}

/// <summary>
/// Internal test sample class for individual counter push testing scenarios.
/// </summary>
/// <typeparam name="TBehavior">The behavior type that defines server configuration and running logic.</typeparam>
file class PushSample<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PushSample{TBehavior}"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    public PushSample(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Runs the individual counter push test scenario.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    public async Task RunAsync()
    {
        this.Trace("get client");
        var log = new ConcurrentQueue<int>();
        await using var client = await GetClient();

        // act
        this.Trace("listen for while");
        using (var _ = client.Demo.ListenCounter().Subscribe(x => log.Enqueue(x.Value)))
        {
            this.Trace("wait for log entries");
            await Expect.ToAsync(() => log.Count.IsGreaterOrEqual(5));
        }

        // assert
        this.Trace("get log snapshot");
        var logData = log.ToArray();

        this.Trace("ensure snapshot entries are ordered");
        logData.OrderBy(x => x).ToArray().SequenceEqual(logData).IsTrue();

        this.Trace("done");
    }
}
