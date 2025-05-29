using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Base;

public abstract class PushTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    protected PushTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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

file class PushSample<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    public PushSample(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
