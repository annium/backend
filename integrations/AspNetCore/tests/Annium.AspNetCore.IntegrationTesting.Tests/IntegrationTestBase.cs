using Annium.AspNetCore.TestServer;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public abstract class IntegrationTestBase : IntegrationTest
{
    protected IWebApplicationFactory AppFactory { get; }

    protected IntegrationTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AppFactory = GetAppFactory<Program>(builder => builder.UseServicePack<TestServicePack>());
    }
}
