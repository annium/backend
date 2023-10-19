using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.Sockets;

public class PushTests : PushTestsBase<Behavior>
{
    public PushTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Counter()
    {
        this.Trace("start");

        await Counter_Base();

        this.Trace("done");
    }
}