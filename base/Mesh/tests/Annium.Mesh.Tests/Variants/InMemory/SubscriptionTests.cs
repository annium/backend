using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Tests for subscription messaging functionality using InMemory transport.
/// </summary>
public class SubscriptionTests : SubscriptionTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public SubscriptionTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests subscription messaging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Subscription()
    {
        this.Trace("start");

        await Subscription_Base();

        this.Trace("done");
    }
}
