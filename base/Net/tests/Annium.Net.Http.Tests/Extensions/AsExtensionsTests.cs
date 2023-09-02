using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http.Internal;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests.Extensions;

public class AsExtensionsTests : TestBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly IHttpContentSerializer _serializer;

    public AsExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _httpRequestFactory = Get<IHttpRequestFactory>();
        _serializer = Get<IHttpContentSerializer>();
    }

    [Fact]
    public async Task AsResult()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (_, response) =>
        {
            // await response.OutputStream.WriteAsync();
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Timeout(TimeSpan.FromMilliseconds(50))
            .RunAsync();

        // assert
        response.IsSuccess.IsFalse();
        response.IsFailure.IsTrue();
        response.StatusCode.Is(HttpStatusCode.GatewayTimeout);

        this.Trace("done");
    }
}