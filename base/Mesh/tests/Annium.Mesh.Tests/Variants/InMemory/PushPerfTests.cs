using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

public class PushPerfTests : PushTestsBase<Behavior>
{
    public PushPerfTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Counter(int index)
    {
        this.Trace("start {index}", index);

        await Counter_Base();

        this.Trace("done {index}", index);
    }

    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Counter_Bundle(int index)
    {
        this.Trace("start {index}", index);

        await Counter_Bundle_Base();

        this.Trace("done {index}", index);
    }

    public static IEnumerable<object[]> GetPerfRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });
}
