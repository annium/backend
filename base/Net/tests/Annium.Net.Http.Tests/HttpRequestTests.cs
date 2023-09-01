using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests;

public class HttpRequestTests : TestBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public HttpRequestTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _httpRequestFactory = Get<IHttpRequestFactory>();
    }

    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        // act
        this.Trace("send text");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").RunAsync();

        // assert
        response.StatusCode.Is(HttpStatusCode.ServiceUnavailable);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Echo()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (request, response) =>
        {
            await request.InputStream.CopyToAsync(response.OutputStream);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Post("/")
            .StringContent(message)
            .RunAsync();

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is(message);

        this.Trace("done");
    }

    private IAsyncDisposable RunServer(
        Func<HttpListenerRequest, HttpListenerResponse, Task> handle
    )
    {
        return RunServerBase(async (ctx, _, _) =>
        {
            this.Trace("start");

            await handle(ctx.Request, ctx.Response);

            this.Trace("done");
        });
    }
}