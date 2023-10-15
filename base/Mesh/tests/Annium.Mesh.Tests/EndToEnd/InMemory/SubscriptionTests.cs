using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.EndToEnd.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Mesh.Tests.EndToEnd.InMemory;

public class SubscriptionTests : SubscriptionTestsBase<Behavior>
{
    public SubscriptionTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Subscription()
    {
        this.Trace("start");

        await Subscription_Base();

        this.Trace("done");
    }
}