using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.InMemory;

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
