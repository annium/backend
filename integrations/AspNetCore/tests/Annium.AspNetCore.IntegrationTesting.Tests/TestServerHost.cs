using System.Threading.Tasks;
using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public class TestServerHost : TestHost<Startup>, IAsyncLifetime
{
    public TestServerHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
