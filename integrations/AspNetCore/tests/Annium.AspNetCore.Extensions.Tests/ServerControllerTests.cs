using System;
using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.IntegrationTesting.Http;
using Annium.AspNetCore.TestServer.Controllers;
using Annium.Data.Operations;
using Annium.Data.Operations.Serialization.Json;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Integration tests for ServerController functionality
/// </summary>
public class ServerControllerTests : TestBase
{
    /// <summary>
    /// The test host started by each test body; bound before the HTTP request factory is resolved.
    /// </summary>
    private ITestHost _testHost = null!;

    /// <summary>
    /// Initializes a new instance of the ServerControllerTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public ServerControllerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // Registrations must happen before InitializeAsync freezes the container, so they live here
        // in the constructor rather than the test body. The factory binds to the host lazily.
        this.RegisterHttpRequestFactory(() => _testHost, true);
        Register(container => container.AddSerializers().WithJson(opts => opts.ConfigureForOperations()));
    }

    /// <summary>
    /// Tests that command endpoint returns BadRequest status when command validation fails
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_BadRequest_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Post("/command")
            .JsonContent(new DemoCommand { IsOk = false })
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.BadRequest);
        response.Data.IsEqual(Result.Create().Error("Not ok"));
    }

    /// <summary>
    /// Tests that command endpoint returns OK status when command is valid
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_Ok_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Post("/command")
            .JsonContent(new DemoCommand { IsOk = true })
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Data.IsEqual(Result.Create());
    }

    /// <summary>
    /// Tests that command endpoint returns Forbidden status when handler reports a forbidden status
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_Forbidden_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Post("/command/forbidden")
            .JsonContent(new DemoForbiddenCommand())
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.Forbidden);
        response.Data.IsEqual(Result.Create().Error("Forbidden"));
    }

    /// <summary>
    /// Tests that command endpoint returns Conflict status when handler reports a conflict status
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_Conflict_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Post("/command/conflict")
            .JsonContent(new DemoConflictCommand())
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.Conflict);
        response.Data.IsEqual(Result.Create().Error("Conflict"));
    }

    /// <summary>
    /// Tests that command endpoint returns InternalServerError status when handler reports an
    /// uncaught-error status, which the HTTP status pipe handler maps to a ServerException
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_ServerError_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Post("/command/server-error")
            .JsonContent(new DemoServerErrorCommand())
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.InternalServerError);
        response.Data.IsEqual(Result.Create().Error("Server error"));
    }

    /// <summary>
    /// Tests that command endpoint returns InternalServerError status and an uncaught-error payload
    /// carrying the exception's own text when the handler throws a plain, unmapped exception
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Command_UnhandledException_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Post("/command/throw")
            .JsonContent(new DemoThrowingCommand())
            .AsResponseAsync<IResult>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.InternalServerError);
        // Data is always populated here: the middleware writes a serialized error body for every 500 response.
        var data = response.Data!;
        data.HasErrors.IsTrue();
        var error = data.PlainErrors.At(0);
        error.IsContaining(nameof(InvalidOperationException));
        error.IsContaining("boom");
    }

    /// <summary>
    /// Tests that query endpoint returns NotFound status when resource is not found
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Query_NotFound_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/query")
            .Param(nameof(DemoQuery.Q), 0)
            .AsResponseAsync<IResult<DemoResponse>>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.NotFound);
        response.Data.IsEqual(Result.Create(default(DemoResponse)).Error("Not found"));
    }

    /// <summary>
    /// Tests that query endpoint returns OK status with data when resource is found
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Query_Ok_Works()
    {
        // arrange
        await using var testHost = await new TestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/query")
            .Param(nameof(DemoQuery.Q), 1)
            .AsResponseAsync<IResult<DemoResponse>>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Data.IsEqual(Result.Create(new DemoResponse { X = 1 }));
    }
}
