using System.Threading.Tasks;
using Annium.Logging;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Base;

/// <summary>
/// Base class for event-based mesh tests, providing common functionality for testing event handling scenarios.
/// </summary>
/// <typeparam name="TBehavior">The behavior type that defines server configuration and running logic.</typeparam>
public abstract class EventTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventTestsBase{TBehavior}"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    protected EventTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Base test method for analytics event testing (currently commented out).
    /// </summary>
    /// <returns>A completed task.</returns>
    protected Task Analytics_Base()
    {
        this.Trace("start");

        // // arrange
        // this.Trace("get client");
        // var log = Get<SharedDataContainer>().Log;
        // var range = Enumerable.Range(0, 500).Select(x => x.ToString()).ToArray();
        // await using var client = await GetClient();
        //
        // // act
        // Parallel.ForEach(range, x =>
        // {
        //     this.Trace("send event");
        //     client.Demo.Analytics(new AnalyticEvent(x));
        //     this.Trace("event sent");
        // });
        //
        // // assert
        // this.Trace("ensure all events arrived");
        // await Expect.To(() => log.Count.Is(range.Length), 2_000);
        // var snapshot = log.ToHashSet();
        // snapshot.Has(range.Length);
        // foreach (var x in range)
        //     snapshot.Contains(x).IsTrue();

        this.Trace("done");

        return Task.CompletedTask;
    }
}
