using System.Threading.Tasks;
using Annium.AspNetCore.TestServer;
using Annium.AspNetCore.TestServer.Components;
using Annium.Net.Http;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class HttpTest : IntegrationTest
    {
        private IWebApplicationFactory AppFactory => GetAppFactory<Startup>(
            builder => builder.UseServicePack<ServicePack>()
        );

        private IHttpRequest Http => AppFactory.GetHttpRequest();

        [Fact]
        public async Task True_IsTrue()
        {
            // arrange
            var value = "custom value";
            var sharedDataContainer = AppFactory.Resolve<SharedDataContainer>();
            sharedDataContainer.Value = value;

            // act
            var result = await Http.Get("/").AsResultAsync<string>();

            // assert
            result.IsOk.IsTrue();
            result.Data.Is(value);
        }
    }
}