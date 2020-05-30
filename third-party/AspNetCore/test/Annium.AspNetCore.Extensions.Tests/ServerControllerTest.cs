using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Net.Http;
using Annium.Testing;
using Demo.AspNetCore;
using Demo.AspNetCore.Controllers;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests
{
    public class ServerControllerTest : IntegrationTest
    {
        private IHttpRequest http => GetRequest<Startup>(
            builder => builder.UseServicePack<ServicePack>()
        );

        [Fact]
        public async Task Command_BadRequest_Works()
        {
            // act
            var response = await http.Post("/command").JsonContent(new DemoCommand { IsOk = false }).AsResponseResultAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.BadRequest);
            response.Data.IsEqual(Result.New().Error("Not ok"));
        }

        [Fact]
        public async Task Command_Ok_Works()
        {
            // act
            var response = await http.Post("/command").JsonContent(new DemoCommand { IsOk = true }).AsResponseAsync<IResult>();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
            response.Data.IsEqual(Result.New());
        }

        [Fact]
        public async Task Query_NotFound_Works()
        {
            // act
            var response = await http.Get("/query").Param(nameof(DemoQuery.Q), 0).AsResponseAsync<IResult<DemoResponse>>();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.NotFound);
            response.Data.IsEqual(Result.New(default(DemoResponse)).Error("Not found"));
        }

        [Fact]
        public async Task Query_Ok_Works()
        {
            // act
            var response = await http.Get("/query").Param(nameof(DemoQuery.Q), 1).AsResponseAsync<IResult<DemoResponse>>();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
            response.Data.IsEqual(Result.New(new DemoResponse { X = 1 }));
        }

        private string Serialize(object obj) => JsonSerializer.Serialize(
            obj,
            new JsonSerializerOptions().ConfigureForOperations()
        );
    }
}