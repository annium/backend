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

public class ServerControllerTest : IntegrationTest
{
    private IHttpRequest Http => GetAppFactory<Program>(
        builder => builder.UseServicePack<TestServicePack>()
    ).GetHttpRequest();

    [Fact]
    public async Task Command_BadRequest_Works()
    {
        // act
        var response = await Http.Post("/command").JsonContent(new DemoCommand { IsOk = false }).AsResponseResultAsync();

        // assert
        response.StatusCode.Is(HttpStatusCode.BadRequest);
        response.Data.IsEqual(Result.New().Error("Not ok"));
    }

    [Fact]
    public async Task Command_Ok_Works()
    {
        // act
        var response = await Http.Post("/command").JsonContent(new DemoCommand { IsOk = true }).AsResponseAsync<IResult>();

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Data.IsEqual(Result.New());
    }

    [Fact]
    public async Task Query_NotFound_Works()
    {
        // act
        var response = await Http.Get("/query").Param(nameof(DemoQuery.Q), 0).AsResponseAsync<IResult<DemoResponse>>();

        // assert
        response.StatusCode.Is(HttpStatusCode.NotFound);
        response.Data.IsEqual(Result.New(default(DemoResponse)).Error("Not found"));
    }

    [Fact]
    public async Task Query_Ok_Works()
    {
        // act
        var response = await Http.Get("/query").Param(nameof(DemoQuery.Q), 1).AsResponseAsync<IResult<DemoResponse>>();

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Data.IsEqual(Result.New(new DemoResponse { X = 1 }));
    }
}