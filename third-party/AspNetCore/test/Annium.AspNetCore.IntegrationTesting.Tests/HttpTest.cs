using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.TestServer;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class HttpTest : IntegrationTest
    {
        private IHttpRequest Http => GetHttpRequest<Startup>(
            builder => builder.UseServicePack<ServicePack>()
        );

        [Fact]
        public async Task True_IsTrue()
        {
            // act
            var response = await Http.Get("/").RunAsync();

            // assert
            response.StatusCode.IsEqual(HttpStatusCode.OK);
        }
    }
}