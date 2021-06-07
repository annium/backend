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
    public class WebSocketTest : IntegrationTestBase
    {
        private Task<TestServerTestClient> GetClient() => AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");

        [Fact]
        public async Task RequestResponse_Works()
        {
            // arrange
            var client = await GetClient();

            // act
            var response = await client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
        }

        [Fact]
        public async Task Subscription_Works()
        {
            // arrange
            var client = await GetClient();
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
            this.Trace(() => "Wait for 4 log entries");
            await Wait.UntilAsync(() => serverLog.Count == 4 && clientLog.Count == 2);

            s1.Dispose();
            s2.Dispose();

            // wait for cancellation entries
            this.Trace(() => "Wait for 6 log entries");
            await Wait.UntilAsync(() => serverLog.Count == 6);

            // assert
            serverLog.Has(6);
            var expectedServerLog = new[]
            {
                "first init: abc",
                "second init: def",
                "first msg",
                "second msg",
                "first canceled",
                "second canceled",
            };
            foreach (var entry in expectedServerLog)
                serverLog.Contains(entry).IsTrue();

            clientLog.Has(2);
            var expectedClientLog = new[]
            {
                "first msg",
                "second msg",
            };
            foreach (var entry in expectedClientLog)
                clientLog.Contains(entry).IsTrue();
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        // ReSharper disable once xUnit1026
        public async Task PerformanceTest(object _)
        {
            Log.SetTestMode();
            this.Trace(() => "get client");
            var client = await AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");
            this.Trace(() => "dispose client");
            await client.DisposeAsync();
            this.Trace(() => "done");
        }

        private static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 20).Select(x => new object[] { x });
    }
}