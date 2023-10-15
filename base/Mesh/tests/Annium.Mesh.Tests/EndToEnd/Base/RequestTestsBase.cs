using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Logging;
using Annium.Mesh.Tests.System.Domain;
using Annium.Testing;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.EndToEnd.Base;

public abstract class RequestTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : IBehavior
{
    protected RequestTestsBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

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
}