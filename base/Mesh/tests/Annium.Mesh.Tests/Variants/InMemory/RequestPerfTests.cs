using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Performance tests for request-response messaging functionality using InMemory transport.
/// </summary>
public class RequestPerfTests : RequestTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestPerfTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RequestPerfTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests echo request-response messaging performance.
    /// </summary>
    /// <param name="index">The test iteration index.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Echo(int index)
    {
        this.Trace("start {index}", index);

        await Echo_Base();

        this.Trace("done {index}", index);
    }

    /// <summary>
    /// Tests bundled echo request-response messaging performance.
    /// </summary>
    /// <param name="index">The test iteration index.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Echo_Bundle(int index)
    {
        this.Trace("start {index}", index);

        await Echo_Bundle_Base();

        this.Trace("done {index}", index);
    }

    /// <summary>
    /// Gets the performance test range data.
    /// </summary>
    /// <returns>A collection of test data for performance iterations.</returns>
    public static IEnumerable<object[]> GetPerfRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });
}
