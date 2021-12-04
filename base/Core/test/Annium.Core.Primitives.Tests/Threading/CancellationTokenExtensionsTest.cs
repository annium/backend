using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives.Threading;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests.Threading;

public class CancellationTokenExtensionsTest
{
    [Fact]
    public async Task CancellationToken_Await_Works()
    {
        // arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10);

        // act
        await cts.Token;

        // assert
        cts.Token.IsCancellationRequested.IsTrue();
    }
}