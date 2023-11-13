using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.Variants.InMemory;

public class PushTests : PushTestsBase<Behavior>
{
    public PushTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task Counter()
    {
        this.Trace("start");

        await Counter_Base();

        this.Trace("done");
    }
}
