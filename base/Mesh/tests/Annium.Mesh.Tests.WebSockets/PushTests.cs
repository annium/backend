using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.WebSockets;

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

        await Push_Base();

        this.Trace("done");
    }
}