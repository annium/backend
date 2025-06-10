using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.WebSockets;

/// <summary>
/// Performance tests for subscription messaging functionality using WebSockets transport.
/// </summary>
public class SubscriptionPerfTests : SubscriptionTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionPerfTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public SubscriptionPerfTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests subscription messaging performance.
    /// </summary>
    /// <param name="index">The test iteration index.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Subscription(int index)
    {
        this.Trace("start {index}", index);

        await Subscription_Base();

        this.Trace("done {index}", index);
    }

    /// <summary>
    /// Gets the performance test range data.
    /// </summary>
    /// <returns>A collection of test data for performance iterations.</returns>
    public static IEnumerable<object[]> GetPerfRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });
}
