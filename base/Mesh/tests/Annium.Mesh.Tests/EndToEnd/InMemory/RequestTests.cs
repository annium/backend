using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.EndToEnd.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.EndToEnd.InMemory;

public class RequestTests : RequestTestsBase<Behavior>
{
    public RequestTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Echo()
    {
        this.Trace("start");

        await Echo_Base();

        this.Trace("done");
    }
}