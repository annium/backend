using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Sockets;

/// <summary>
/// Tests for event messaging functionality using Sockets transport.
/// </summary>
public class EventTests : EventTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public EventTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests analytics event messaging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Analytics()
    {
        this.Trace("start");

        await Analytics_Base();

        this.Trace("done");
    }
}
