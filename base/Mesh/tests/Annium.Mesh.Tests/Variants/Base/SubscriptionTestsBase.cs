using System.Threading.Tasks;
using Annium.Logging;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Base;

/// <summary>
/// Base class for subscription-based mesh tests, providing common functionality for testing subscription scenarios.
/// </summary>
/// <typeparam name="TBehavior">The behavior type that defines server configuration and running logic.</typeparam>
public abstract class SubscriptionTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionTestsBase{TBehavior}"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    protected SubscriptionTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Base test method for subscription functionality (currently commented out).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Subscription_Base()
    {
        this.Trace("start");

        await Task.CompletedTask;
        // // arrange
        // this.Trace("get client");
        // await using var client = await GetClient();
        //
        // var serverLog = Get<SharedDataContainer>().Log;
        // var clientLog = new ConcurrentQueue<string>();
        //
        // void ClientLog(string value)
        // {
        //     this.Trace<string>("client log: {value}", value);
        //     clientLog.Enqueue(value);
        // }
        //
        // var cts = new CancellationTokenSource();
        //
        // // act
        // this.Trace("subscribe first");
        // var o1 = await client.Demo.SubscribeFirstAsync(new FirstSubscriptionInit { Param = "abc" }, cts.Token).GetData();
        //
        // this.Trace("schedule first completion tracking");
        // var os1 = o1.Subscribe(ClientLog);
        // this.Trace("first subscribed");
        //
        // this.Trace("subscribe second");
        // var o2 = await client.Demo.SubscribeSecondAsync(new SecondSubscriptionInit { Param = "def" }, cts.Token).GetData();
        //
        // this.Trace("schedule second completion tracking");
        // var os2 = o2.Subscribe(ClientLog);
        // this.Trace("second subscribed");
        //
        // // wait for init and msg entries
        // this.Trace("wait for init and msg log entries");
        // await Expect.To(() =>
        // {
        //     this.Trace("assert client/server log");
        //     serverLog.Has(6);
        //     clientLog.Has(4);
        // }, 2000);
        //
        // this.Trace("dispose subscriptions");
        // cts.Cancel();
        // os1.Dispose();
        // os2.Dispose();
        //
        // // wait for cancellation entries
        // this.Trace("wait for cancellation log entries");
        // await Expect.To(() => serverLog.Has(8), 2000);
        //
        // // assert
        // this.Trace("verify log");
        // serverLog.Has(8);
        // // filter server log and ensure messages order
        // var expectedServerFirstLog = new[]
        // {
        //     "first init: abc",
        //     "first msg1",
        //     "first msg2",
        //     "first canceled",
        // };
        // serverLog.Where(x => x.StartsWith("first")).ToArray().IsEqual(expectedServerFirstLog);
        // var expectedServerSecondLog = new[]
        // {
        //     "second init: def",
        //     "second msg1",
        //     "second msg2",
        //     "second canceled",
        // };
        // serverLog.Where(x => x.StartsWith("second")).ToArray().IsEqual(expectedServerSecondLog);
        //
        // clientLog.Has(4);
        // // filter server log and ensure messages order
        // var expectedClientFirstLog = new[]
        // {
        //     "first msg1",
        //     "first msg2",
        // };
        // clientLog.Where(x => x.StartsWith("first")).ToArray().IsEqual(expectedClientFirstLog);
        // var expectedClientSecondLog = new[]
        // {
        //     "second msg1",
        //     "second msg2",
        // };
        // clientLog.Where(x => x.StartsWith("second")).ToArray().IsEqual(expectedClientSecondLog);

        this.Trace("done");
    }
}
