using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Event tests for in-memory mesh transport implementation.
/// </summary>
public class EventTests : EventTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventTests"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    public EventTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests analytics event handling using in-memory transport.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Analytics()
    {
        this.Trace("start");

        await Analytics_Base();

        this.Trace("done");
    }
}
