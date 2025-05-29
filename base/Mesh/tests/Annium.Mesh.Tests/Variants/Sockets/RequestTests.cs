using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Sockets;

public class RequestTests : RequestTestsBase<Behavior>
{
    public RequestTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task Echo()
    {
        this.Trace("start");

        await Echo_Base();

        this.Trace("done");
    }
}
