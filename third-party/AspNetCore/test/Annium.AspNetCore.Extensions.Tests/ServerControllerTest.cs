using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Data.Operations.Serialization;
using Annium.Net.Http;
using Annium.Testing;
using Demo.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Annium.AspNetCore.Extensions.Tests
{
    public class ServerControllerTest : IntegrationTest
    {
        public ServerControllerTest() : base(container => container.UseServicePack<Demo.AspNetCore.ServicePack>()) { }

        private IRequest http => GetRequest<Startup>();

        [Fact]
        public async Task Conlfict_Works()
        {
            // arrange
            var expected = Serialize(Result.Failure());

            // act
            var response = await http.Get("/conflict").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.Conflict);
            (await response.Content.ReadAsStringAsync()).IsEqual(expected);
        }

        [Fact]
        public async Task Created_Works()
        {
            // act
            var response = await http.Get("/created").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.Created);
            (await response.Content.ReadAsStringAsync()).IsEqual("created");
        }

        [Fact]
        public async Task BadRequest_Works()
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
        public async Task Forbidden_Works()
        {
            // arrange
            var expected = Serialize(Result.Failure());

            // act
            var response = await http.Get("/forbidden").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.Forbidden);
            (await response.Content.ReadAsStringAsync()).IsEqual(expected);
        }

        [Fact]
        public async Task NotFound_Works()
        {
            // arrange
            var expected = Serialize(Result.Failure());

            // act
            var response = await http.Get("/not-found").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.NotFound);
            (await response.Content.ReadAsStringAsync()).IsEqual(expected);
        }

        [Fact]
        public async Task ServerError_Works()
        {
            // arrange
            var expected = Serialize(Result.Failure());

            // act
            var response = await http.Get("/server-error").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.InternalServerError);
            (await response.Content.ReadAsStringAsync()).IsEqual(expected);
        }

        private string Serialize(object obj) => JsonConvert.SerializeObject(
            obj,
            new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
            }
            .ConfigureForOperations()
        );
    }
}