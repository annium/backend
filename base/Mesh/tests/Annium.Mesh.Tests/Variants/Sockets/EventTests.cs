using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.Variants.Sockets;

public class EventTests : EventTestsBase<Behavior>
{
    public EventTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task Analytics()
    {
        this.Trace("start");

        await Analytics_Base();

        this.Trace("done");
    }
}
