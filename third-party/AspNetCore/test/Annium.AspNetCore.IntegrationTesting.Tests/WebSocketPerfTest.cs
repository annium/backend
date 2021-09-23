using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class WebSocketPerfTest : IntegrationTestBase
    {
        private Task<TestServerTestClient> GetClient() => AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");

        [Theory]
        [MemberData(nameof(GetRange))]
        public async Task PerfRequestResponse_Works(int index)
        {
            Log.SetTestMode();
            Console.WriteLine($"{nameof(PerfRequestResponse_Works)}#{index} - start");

            // arrange
            await using var client = await GetClient();

            // act
            var response = await client.Demo.EchoAsync(new EchoRequest("Hi"));

            // assert
            response.Status.Is(OperationStatus.Ok);
            response.Data.Is("Hi");
            Console.WriteLine($"{nameof(PerfRequestResponse_Works)}#{index} - done");
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        public async Task PerfRequestResponseBundle_Works(int index)
        {
            Log.SetTestMode();
            Console.WriteLine($"{nameof(PerfRequestResponseBundle_Works)}#{index} - start");

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

            Console.WriteLine($"{nameof(PerfRequestResponseBundle_Works)}#{index} - start");
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        public async Task PerfSubscription_Works(int index)
        {
            Log.SetTestMode();
            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - start");

            // arrange
            await using var client = await GetClient();
            var serverLog = AppFactory.Resolve<SharedDataContainer>().Log;
            var clientLog = new ConcurrentQueue<string>();

            void ClientLog(string value)
            {
                Console.WriteLine($"client log: {value}");
                clientLog.Enqueue(value);
            }

            var cts = new CancellationTokenSource();

            // act
            var o1 = await client.Demo.SubscribeFirstAsync(new FirstSubscriptionInit { Param = "abc" }, cts.Token).GetData();
            var os1 = o1.Subscribe(ClientLog);
            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - first subscribed");
            var o2 = await client.Demo.SubscribeSecondAsync(new SecondSubscriptionInit { Param = "def" }, cts.Token).GetData();
            var os2 = o2.Subscribe(ClientLog);
            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - second subscribed");
            // wait for init and msg entries
            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - wait for init and msg log entries");
            await Wait.UntilAsync(() => serverLog.Count == 6 && clientLog.Count == 4);

            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - dispose subscriptions");
            cts.Cancel();
            os1.Dispose();
            os2.Dispose();
            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - await subscriptions");
            await o1.WhenCompleted();
            await o2.WhenCompleted();

            // wait for cancellation entries
            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - wait for cancellation log entries");
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

            Console.WriteLine($"{nameof(PerfSubscription_Works)}#{index} - done");
        }

        [Theory]
        [MemberData(nameof(GetRange))]
        // ReSharper disable once xUnit1026
        public async Task PerfConnection_Works(int index)
        {
            Log.SetTestMode();
            Console.WriteLine($"{nameof(PerfConnection_Works)}#{index} - start");

            Console.WriteLine("get client");
            var client = await AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");
            Console.WriteLine("dispose client");
            await client.DisposeAsync();

            Console.WriteLine($"{nameof(PerfRequestResponseBundle_Works)}#{index} - done");
        }

        private static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 100).Select(x => new object[] { x });
    }
}