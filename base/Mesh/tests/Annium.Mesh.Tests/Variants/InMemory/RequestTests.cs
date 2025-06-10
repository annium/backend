using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Tests for request-response messaging functionality using InMemory transport.
/// </summary>
public class RequestTests : RequestTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RequestTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests echo request-response messaging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Echo()
    {
        this.Trace("start");

        await Echo_Base();

        this.Trace("done");
    }
}
