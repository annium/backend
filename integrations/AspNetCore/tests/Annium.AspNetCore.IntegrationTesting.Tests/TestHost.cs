using System.Threading.Tasks;
using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

internal class TestHost : TestHostBase<Program>
{
    public TestHost(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        //
    }

    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
    }

    protected override ValueTask HandleStartAsync()
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask HandleStopAsync()
    {
        return ValueTask.CompletedTask;
    }
}
