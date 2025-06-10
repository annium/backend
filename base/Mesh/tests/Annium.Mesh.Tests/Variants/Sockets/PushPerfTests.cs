using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Tests.Variants.Base;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Sockets;

/// <summary>
/// Performance tests for push messaging functionality using Sockets transport.
/// </summary>
public class PushPerfTests : PushTestsBase<Behavior>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PushPerfTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public PushPerfTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests counter push messaging performance.
    /// </summary>
    /// <param name="index">The test iteration index.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Counter(int index)
    {
        this.Trace("start {index}", index);

        await Counter_Base();

        this.Trace("done {index}", index);
    }

    /// <summary>
    /// Tests bundled counter push messaging performance.
    /// </summary>
    /// <param name="index">The test iteration index.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Theory]
    [MemberData(nameof(GetPerfRange))]
    public async Task Counter_Bundle(int index)
    {
        this.Trace("start {index}", index);

        await Counter_Bundle_Base();

        this.Trace("done {index}", index);
    }

    /// <summary>
    /// Gets the performance test range data.
    /// </summary>
    /// <returns>A collection of test data for performance iterations.</returns>
    public static IEnumerable<object[]> GetPerfRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });
}
