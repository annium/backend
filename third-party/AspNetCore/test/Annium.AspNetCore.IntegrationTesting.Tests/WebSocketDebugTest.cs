using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient.Clients;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Core.Internal;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Data.Operations;
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
            Console.WriteLine($"{nameof(DebugRequestResponse_Works)} - start");

            // arrange
            await using var client = await GetClient();

            // act
            var response = await client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
            Console.WriteLine($"{nameof(DebugRequestResponse_Works)} - done");
        }

        [Fact]
        public async Task DebugSubscription_Works()
        {
            Log.SetTestMode();
            Console.WriteLine($"{nameof(DebugSubscription_Works)} - start");

            // arrange
            await using var client = await GetClient();
            var serverLog = AppFactory.Resolve<SharedDataContainer>().Log;
            var clientLog = new ConcurrentQueue<string>();

            // act
            var o1 = await client.Demo.SubscribeFirstAsync(new FirstSubscriptionInit { Param = "abc" }).GetData();
            o1.Subscribe(x => { clientLog.Enqueue(x); });
            Console.WriteLine($"{nameof(DebugSubscription_Works)} - first subscribed");
            var o2 = await client.Demo.SubscribeSecondAsync(new SecondSubscriptionInit { Param = "def" }).GetData();
            o2.Subscribe(x => { clientLog.Enqueue(x); });
            Console.WriteLine($"{nameof(DebugSubscription_Works)} - second subscribed");
            // wait for init and msg entries
            Console.WriteLine($"{nameof(DebugSubscription_Works)} - wait for init and msg log entries");
            await Wait.UntilAsync(() => serverLog.Count == 6 && clientLog.Count == 4);

            Console.WriteLine($"{nameof(DebugSubscription_Works)} - dispose subscriptions");
            await o1;
            await o2;

            // wait for cancellation entries
            Console.WriteLine($"{nameof(DebugSubscription_Works)} - wait for cancellation log entries");
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

            Console.WriteLine($"{nameof(DebugSubscription_Works)} - done");
        }
    }
}