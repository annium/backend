using System.Net;
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
    /// The test host started by the test body; bound before the HTTP request factory is resolved.
    /// </summary>
    private ITestHost _testHost = null!;

    /// <summary>
    /// Initializes a new instance of the HttpTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public HttpTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // Registrations must happen before InitializeAsync freezes the container, so they live here
        // in the constructor rather than the test body. The factory binds to the host lazily.
        this.RegisterHttpRequestFactory(() => _testHost, true);
        Register(container => container.AddSerializers().WithJson(opts => opts.ConfigureForOperations()));
    }

    /// <summary>
    /// Tests that HTTP requests work correctly with shared data containers
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task SimpleRequest_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        const string value = "custom value";
        var sharedDataContainer = testHost.Get<SharedDataContainer>();
        sharedDataContainer.Value = value;

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

    /// <summary>
    /// Tests that a request to a route with no matching endpoint surfaces as a 404 through the
    /// <see cref="Annium.Net.Http.AsResponseExtensions.AsResponseAsync{T}(Annium.Net.Http.IHttpRequest, System.Threading.CancellationToken)" /> path.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Request_NonExistentRoute_ReturnsNotFound()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/this-route-does-not-exist")
            .AsResponseAsync<string>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.NotFound);
    }
}
