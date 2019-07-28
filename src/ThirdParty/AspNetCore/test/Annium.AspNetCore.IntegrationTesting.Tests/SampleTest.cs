using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.Demo;
using Annium.Net.Http;
using Annium.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class SampleTest : IntegrationTest
    {
        private IRequest http => GetRequest<Startup<ServicePack>>();

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