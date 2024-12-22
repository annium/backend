using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Tests.System.Domain;
using Annium.Testing;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.Variants.Base;

public abstract class RequestTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    protected RequestTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    protected async Task Echo_Base()
    {
        this.Trace("start");

        // arrange
        var message = "test message";

        this.Trace("get client");
        await using var client = await GetClient();

        // act
        this.Trace("send request");
        var response = await client.Demo.EchoAsync(new EchoRequest(message));

        // assert
        this.Trace("validate response");
        response.Status.Is(OperationStatus.Ok);
        response.Data.Is(message);

        this.Trace("done");
    }

    protected async Task Echo_Bundle_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("get client");
        await using var client = await GetClient();
        var responses = new ConcurrentBag<string?>();
        var range = Enumerable.Range(0, 500).Select(x => x.ToString()).ToArray();

        // act
        await Task.WhenAll(
            range.Select(async x =>
            {
                this.Trace("send request");
                var response = await client.Demo.EchoAsync(new EchoRequest(x)).GetDataAsync();
                this.Trace("add response");
                responses.Add(response);
            })
        );

        // assert
        var set = new HashSet<string?>(responses);
        set.Has(range.Length);
        foreach (var x in range)
            set.Contains(x).IsTrue();

        this.Trace("done");
    }
}
