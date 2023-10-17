using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.Base;

public abstract class PushTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : IBehavior
{
    protected PushTestsBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected async Task Push_Base()
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
            this.Trace("wait for a while");
            await Task.Delay(200);
        }

        // assert
        this.Trace("get log snapshot");
        var logData = log.ToArray();

        this.Trace("ensure snapshot has log entries");
        logData.Length.IsGreater(0);

        this.Trace("ensure snapshot entries are ordered");
        logData.OrderBy(x => x).ToArray().IsEqual(logData);

        this.Trace("done");
    }
}