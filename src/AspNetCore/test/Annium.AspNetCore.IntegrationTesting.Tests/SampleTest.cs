using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.Demo;
using Annium.Extensions.Net.Http;
using Annium.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class SampleTest : IntegrationTest<Startup<ServicePack>>
    {
        public SampleTest()
        {
            Configure(request => request);
        }

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