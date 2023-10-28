using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.InMemory;

public class SubscriptionPerfTests : SubscriptionTestsBase<Behavior>
{
    public SubscriptionPerfTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Subscription(int index)
    {
        this.Trace("start {index}", index);

        await Subscription_Base();

        this.Trace("done {index}", index);
    }

    public static IEnumerable<object[]> GetPerfRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });
}
