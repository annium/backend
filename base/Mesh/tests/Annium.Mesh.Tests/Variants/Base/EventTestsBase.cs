using System.Threading.Tasks;
using Annium.Logging;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Base;

public abstract class EventTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    protected EventTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
