using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests;

public class HttpRequestConfigureInterceptTests : TestBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public HttpRequestConfigureInterceptTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _httpRequestFactory = Get<IHttpRequestFactory>();
    }

    [Fact]
    public async Task Configure_Request()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (request, response) =>
        {
            var data = Encoding.UTF8.GetBytes(request.Url.NotNull().Query);
            await response.OutputStream.WriteAsync(data);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Configure(req => req.Param("x", "a"))
            .RunAsync();

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is("?x=a");

        this.Trace("done");
    }

    [Fact]
    public async Task Configure_Request_Options()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (request, response) =>
        {
            var data = Encoding.UTF8.GetBytes(request.Url.NotNull().Query);
            await response.OutputStream.WriteAsync(data);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Configure((req, opts) => req.Param("dup", opts.Params["x"]))
            .Param("x", 1)
            .RunAsync();

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is("?x=1&dup=1");

        this.Trace("done");
    }

    [Fact]
    public async Task Intercept_Next()
    {
        this.Trace("start");

        // arrange
        var message = "demo";
        var log = new List<string>();
        await using var _ = RunServer(async (request, response) =>
        {
            await request.InputStream.CopyToAsync(response.OutputStream);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Intercept(async next =>
            {
                log.Add("before");
                var response = await next();
                log.Add("after");

                return response;
            })
            .StringContent(message)
            .RunAsync();

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is(message);
        log.IsEqual(new[] { "before", "after" });

        this.Trace("done");
    }

    [Fact]
    public async Task Intercept_Next_Request()
    {
        this.Trace("start");

        // arrange
        var message = "demo";
        var log = new List<string>();
        await using var _ = RunServer(async (request, response) =>
        {
            await request.InputStream.CopyToAsync(response.OutputStream);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Intercept(async (next, req) =>
            {
                log.Add($"before {req.Params["x"]}");
                var response = await next();
                log.Add($"after {req.Params["x"]}");

                return response;
            })
            .Param("x", 1)
            .StringContent(message)
            .RunAsync();

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is(message);
        log.IsEqual(new[] { "before 1", "after 1" });

        this.Trace("done");
    }

    [Fact]
    public async Task Intercept_Next_Request_Options()
    {
        this.Trace("start");

        // arrange
        var message = "demo";
        var log = new List<string>();
        await using var _ = RunServer(async (request, response) =>
        {
            await request.InputStream.CopyToAsync(response.OutputStream);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Intercept(async (next, req, opts) =>
            {
                log.Add($"before {req.Params["x"]} {opts.Params["y"]}");
                var response = await next();
                log.Add($"after {req.Params["x"]} {opts.Params["y"]}");

                return response;
            })
            .Param("x", 1)
            .Param("y", 2)
            .StringContent(message)
            .RunAsync();

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is(message);
        log.IsEqual(new[] { "before 1 2", "after 1 2" });

        this.Trace("done");
    }
}