using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Push tests for in-memory mesh transport implementation.
/// </summary>
public class PushTests : PushTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PushTests"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    public PushTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests counter push functionality using in-memory transport.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Counter()
    {
        this.Trace("start");

        await Counter_Base();

        this.Trace("done");
    }
}
