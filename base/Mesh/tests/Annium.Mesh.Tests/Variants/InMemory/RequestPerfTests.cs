using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.Variants.InMemory;

public class RequestPerfTests : RequestTestsBase<Behavior>
{
    public RequestPerfTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Echo(int index)
    {
        this.Trace("start {index}", index);

        await Echo_Base();

        this.Trace("done {index}", index);
    }

    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Echo_Bundle(int index)
    {
        this.Trace("start {index}", index);

        await Echo_Bundle_Base();

        this.Trace("done {index}", index);
    }

    public static IEnumerable<object[]> GetPerfRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });
}
