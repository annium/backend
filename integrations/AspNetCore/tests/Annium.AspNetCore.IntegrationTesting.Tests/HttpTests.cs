using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting.Http;
using Annium.AspNetCore.TestServer.Components;
using Annium.Data.Operations;
using Annium.Data.Operations.Serialization.Json;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Integration tests for HTTP functionality in the integration testing framework
/// </summary>
public class HttpTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the HttpTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public HttpTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that HTTP requests work correctly with shared data containers
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task SimpleRequest_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();

        const string value = "custom value";
        var sharedDataContainer = testHost.Get<SharedDataContainer>();
        sharedDataContainer.Value = value;

        this.RegisterHttpRequestFactory(testHost, true);
        Register(container => container.AddSerializers().WithJson(opts => opts.ConfigureForOperations()));

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var result = await httpRequestFactory
            .New()
            .Get("/")
            .AsAsync<IResult<string>>(ct: TestContext.Current.CancellationToken);

        // assert
        result.IsNotDefault();
        result.IsOk.IsTrue();
        result.Data.Is(value);
    }
}
