using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.IntegrationTesting.WebSocketClient.Clients;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests
{
    public class WebSocketPerfTest : IntegrationTestBase
    {
        private Task<TestServerTestClient> GetClient() => AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");

        [Theory]
        [MemberData(nameof(GetRange))]
        public async Task PerfRequestResponse_Works(int index)
        {
            Log.SetTestMode();
            this.Trace(() => $"start {index}");

            // arrange
            await using var client = await GetClient();

            // act
            var response = await client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
            this.Trace(() => $"done {index}");
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        public async Task PerRequestResponseBundle_Works(int index)
        {
            Log.SetTestMode();
            this.Trace(() => $"start {index}");

            // arrange
            await using var client = await GetClient();
            var responses = new ConcurrentBag<string>();
            var range = Enumerable.Range(0, 500).Select(x => x.ToString()).ToArray();

            // act
            await Task.WhenAll(
                range
                    .Select(async x =>
                    {
                        var response = await client.Demo.EchoAsync(new EchoRequest(x)).GetData();
                        responses.Add(response);
                    })
            );

            // assert
            var set = new HashSet<string>(responses);
            set.Has(range.Length);
            foreach (var x in range)
                set.Contains(x);

            this.Trace(() => $"done {index}");
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        public async Task PerfSubscription_Works(int index)
        {
            Log.SetTestMode();
            this.Trace(() => $"start {index}");

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
            this.Trace(() => "Wait for 6 log entries");
            await Wait.UntilAsync(() => serverLog.Count == 6 && clientLog.Count == 4);

            s1.Dispose();
            s2.Dispose();

            // wait for cancellation entries
            this.Trace(() => "Wait for 8 log entries");
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

            this.Trace(() => $"done {index}");
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        // ReSharper disable once xUnit1026
        public async Task PerfConnection_Works(int index)
        {
            Log.SetTestMode();
            this.Trace(() => $"Run {index}");

            this.Trace(() => "get client");
            var client = await AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");
            this.Trace(() => "dispose client");
            await client.DisposeAsync();

            this.Trace(() => "done");
        }

        private static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 100).Select(x => new object[] { x });
    }
}