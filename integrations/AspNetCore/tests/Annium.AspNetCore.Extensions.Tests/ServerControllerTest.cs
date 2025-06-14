using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.TestServer;
using Annium.AspNetCore.TestServer.Controllers;
using Annium.Data.Operations;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Integration tests for ServerController functionality
/// </summary>
public class ServerControllerTest : IntegrationTest
{
    /// <summary>
    /// Initializes a new instance of the ServerControllerTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public ServerControllerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Gets the HTTP request instance configured for the test server
    /// </summary>
    private IHttpRequest Http =>
        GetAppFactory<Program>(builder => builder.UseServicePack<TestServicePack>()).GetHttpRequest();

    /// <summary>
    /// Tests that command endpoint returns BadRequest status when command validation fails
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_BadRequest_Works()
    {
        // act
        var response = await Http.Post("/command")
            .JsonContent(new DemoCommand { IsOk = false })
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.BadRequest);
        response.Data.IsEqual(Result.New().Error("Not ok"));
    }

    /// <summary>
    /// Tests that command endpoint returns OK status when command is valid
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_Ok_Works()
    {
        // act
        var response = await Http.Post("/command")
            .JsonContent(new DemoCommand { IsOk = true })
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Data.IsEqual(Result.New());
    }

    /// <summary>
    /// Tests that query endpoint returns NotFound status when resource is not found
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Query_NotFound_Works()
    {
        // act
        var response = await Http.Get("/query")
            .Param(nameof(DemoQuery.Q), 0)
            .AsResponseAsync<IResult<DemoResponse>>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.NotFound);
        response.Data.IsEqual(Result.New(default(DemoResponse)).Error("Not found"));
    }

    /// <summary>
    /// Tests that query endpoint returns OK status with data when resource is found
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Query_Ok_Works()
    {
        // act
        var response = await Http.Get("/query")
            .Param(nameof(DemoQuery.Q), 1)
            .AsResponseAsync<IResult<DemoResponse>>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Data.IsEqual(Result.New(new DemoResponse { X = 1 }));
    }
}
