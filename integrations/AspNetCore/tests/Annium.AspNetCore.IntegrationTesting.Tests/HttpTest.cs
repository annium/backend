using System.Threading.Tasks;
using Annium.AspNetCore.TestServer.Components;
using Annium.Data.Operations;
using Annium.Net.Http;
using Annium.Net.Http.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Integration tests for HTTP functionality in the integration testing framework
/// </summary>
public class HttpTest : IntegrationTestBase
{
    /// <summary>
    /// Gets the HTTP request instance for testing
    /// </summary>
    private IHttpRequest Http => AppFactory.GetHttpRequest();

    /// <summary>
    /// Tests that HTTP requests work correctly with shared data containers
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task True_IsTrue()
    {
        // arrange
        var value = "custom value";
        var sharedDataContainer = AppFactory.Resolve<SharedDataContainer>();
        sharedDataContainer.Value = value;

        // act
        var result = await Http.Get("/").AsAsync<IResult<string>>(TestContext.Current.CancellationToken);

        // assert
        result.IsNotDefault();
        result.IsOk.IsTrue();
        result.Data.Is(value);
    }

    /// <summary>
    /// Initializes a new instance of the HttpTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public HttpTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
