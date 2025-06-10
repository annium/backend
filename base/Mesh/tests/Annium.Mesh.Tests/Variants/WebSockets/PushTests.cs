using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.WebSockets;

/// <summary>
/// Tests for push messaging functionality using WebSockets transport.
/// </summary>
public class PushTests : PushTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PushTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public PushTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests counter push messaging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Counter()
    {
        this.Trace("start");

        await Counter_Base();

        this.Trace("done");
    }
}
