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
        private IRequest http => GetRequest<Startup>(
            builder => builder.UseServicePack<ServicePack>()
        );

        [Fact]
        public async Task Command_BadRequest_Works()
        {
            // act
            var response = await http.Post("/command").JsonContent(new DemoCommand { IsOk = false }).AsResponseAsync<IResult>();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.BadRequest);
            response.Data.LabeledErrors.Has(0);
            response.Data.PlainErrors.IsEqual(new[] { "Not ok" });
        }

        [Fact]
        public async Task Command_Ok_Works()
        {
            // act
            var response = await http.Post("/command").JsonContent(new DemoCommand { IsOk = true }).AsResponseAsync<IResult>();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
            response.Data.HasErrors.IsFalse();
        }

        [Fact]
        public async Task Query_NotFound_Works()
        {
            // arrange
            var expected = Serialize(
                Result.New()
                .Error("name", "The Name field is required.")
                .Error("email", "The Email field is required.")
            );

            // act
            var response = await http.Post("/bad-request")
                .JsonContent(new { name = "", email = "" })
                .RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).IsEqual(expected);
        }

        [Fact]
        public async Task Query_Ok_Works()
        {
            // arrange
            var expected = Serialize(
                Result.New()
                .Error("name", "The Name field is required.")
                .Error("email", "The Email field is required.")
            );

            // act
            var response = await http.Post("/bad-request")
                .JsonContent(new { name = "", email = "" })
                .RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).IsEqual(expected);
        }

        private string Serialize(object obj) => JsonSerializer.Serialize(
            obj,
            new JsonSerializerOptions().ConfigureForOperations()
        );
    }
}