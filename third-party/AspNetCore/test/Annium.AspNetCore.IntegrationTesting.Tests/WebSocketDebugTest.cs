using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.IntegrationTesting.WebSocketClient.Clients;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Core.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class WebSocketDebugTest : IntegrationTestBase
    {
        private Task<TestServerTestClient> GetClient() => AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");

        [Fact]
        public async Task RequestResponse_Works()
        {
            Log.SetTestMode();
            this.Trace(() => "start");

            // arrange
            await using var client = await GetClient();

            // act
            var response = await client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
            this.Trace(() => "done");
        }
    }
}