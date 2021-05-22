using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.IntegrationTesting.WebSocketClient;
using Annium.AspNetCore.IntegrationTesting.WebSocketClient.Clients;
using Annium.AspNetCore.TestServer;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class WebSocketTest : IntegrationTest
    {
        private TestServerTestClient Client => GetWebSocketClient<Startup, TestServerTestClient>(
            builder => builder.UseServicePack<ServicePack>(),
            container => container.AddTestServerTestClient(x => x.WithActiveKeepAlive(600)),
            "/ws"
        );

        [Fact]
        public async Task RequestResponse_Works()
        {
            // act
            var response = await Client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
        }
    }
}