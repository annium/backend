using System.Net;
using System.Threading.Tasks;
using Annium.Net.Http;
using Annium.Testing;
using Demo.AspNetCore;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class SampleTest : IntegrationTest
    {
        private IHttpRequest http => GetRequest<Startup>(
            builder => builder.UseServicePack<ServicePack>()
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