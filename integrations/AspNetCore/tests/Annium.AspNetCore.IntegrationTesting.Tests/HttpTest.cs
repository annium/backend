using System.Threading.Tasks;
using Annium.AspNetCore.TestServer.Components;
using Annium.Data.Operations;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public class HttpTest : IntegrationTestBase
{
    private IHttpRequest Http => AppFactory.GetHttpRequest();

    [Fact]
    public async Task True_IsTrue()
    {
        // arrange
        var value = "custom value";
        var sharedDataContainer = AppFactory.Resolve<SharedDataContainer>();
        sharedDataContainer.Value = value;

        // act
        var result = await Http.Get("/").AsAsync<IResult<string>>();

        // assert
        result.IsNotDefault();
        result.IsOk.IsTrue();
        result.Data.Is(value);
    }

    public HttpTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
