using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting;

public abstract class IntegrationTestBase : TestBase, IAsyncLifetime
{
    protected IntegrationTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    public ValueTask InitializeAsync()
    {
        this.Trace("initializing");
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        this.Trace("disposing");
        return ValueTask.CompletedTask;
    }
}
