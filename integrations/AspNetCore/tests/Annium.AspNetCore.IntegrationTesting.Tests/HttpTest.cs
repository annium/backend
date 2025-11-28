using System.Net.Http;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Integration tests for HTTP functionality in the integration testing framework
/// </summary>
public class HttpTest : TestBase
{
    private readonly TestServerHost _testHost;

    /// <summary>
    /// Initializes a new instance of the HttpTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public HttpTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _testHost = new TestServerHost(outputHelper);
    }

    /// <summary>
    /// Tests that HTTP requests work correctly with shared data containers
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Http_Works()
    {
        this.Trace("start");
        using var client = _testHost.Server.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        response.IsSuccessStatusCode.IsTrue();
        this.Trace("done");
    }
}
