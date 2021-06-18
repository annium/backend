using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.IntegrationTesting.WebSocketClient.Clients;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class WebSocketDebugTest : IntegrationTestBase
    {
        private Task<TestServerTestClient> GetClient() => AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");

        [Fact]
        public async Task DebugRequestResponse_Works()
        {
            Log.SetTestMode();
            Console.WriteLine("start");

            // arrange
            await using var client = await GetClient();

            // act
            var response = await client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
            Console.WriteLine("done");
        }

        [Fact]
        public async Task DebugSubscription_Works()
        {
            Log.SetTestMode();
            Console.WriteLine("start");

            // arrange
            await using var client = await GetClient();
            var serverLog = AppFactory.Resolve<SharedDataContainer>().Log;
            var clientLog = new List<string>();

            // act
            var s1 = client.Demo
                .ListenFirst(new FirstSubscriptionInit { Param = "abc" })
                .Subscribe(clientLog.Add);
            var s2 = client.Demo
                .ListenSecond(new SecondSubscriptionInit { Param = "def" })
                .Subscribe(clientLog.Add);
            // wait for init and msg entries
            Console.WriteLine("Wait for 6 log entries");
            await Wait.UntilAsync(() => serverLog.Count == 6 && clientLog.Count == 4);

            s1.Dispose();
            s2.Dispose();

            // wait for cancellation entries
            Console.WriteLine("Wait for 8 log entries");
            await Wait.UntilAsync(() => serverLog.Count == 8);

            // assert
            serverLog.Has(8);
            // filter server log and ensure messages order
            var expectedServerFirstLog = new[]
            {
                "first init: abc",
                "first msg1",
                "first msg2",
                "first canceled",
            };
            serverLog.Where(x => x.StartsWith("first")).ToArray().IsEqual(expectedServerFirstLog);
            var expectedServerSecondLog = new[]
            {
                "second init: def",
                "second msg1",
                "second msg2",
                "second canceled",
            };
            serverLog.Where(x => x.StartsWith("second")).ToArray().IsEqual(expectedServerSecondLog);

            clientLog.Has(4);
            // filter server log and ensure messages order
            var expectedClientFirstLog = new[]
            {
                "first msg1",
                "first msg2",
            };
            clientLog.Where(x => x.StartsWith("first")).ToArray().IsEqual(expectedClientFirstLog);
            var expectedClientSecondLog = new[]
            {
                "second msg1",
                "second msg2",
            };
            clientLog.Where(x => x.StartsWith("second")).ToArray().IsEqual(expectedClientSecondLog);

            Console.WriteLine("done");
        }
    }
}