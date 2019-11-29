using System.Net;
using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Testing;
using Demo.AspNetCore;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class SampleTest : IntegrationTest
    {
        private IRequest http => GetRequest<Startup>(
            builder => builder.UseServicePack<Demo.AspNetCore.ServicePack>()
        );

        [Fact]
        public async Task True_IsTrue()
        {
            // act
            var response = await http.Get("/").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
        }
    }
}