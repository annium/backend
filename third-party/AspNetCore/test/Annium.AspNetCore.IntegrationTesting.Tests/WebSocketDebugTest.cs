using System;
using System.Collections.Concurrent;
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

namespace Annium.AspNetCore.IntegrationTesting.Tests;

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
        Trace("start");

        // arrange
        await using var client = await GetClient();
        var serverLog = AppFactory.Resolve<SharedDataContainer>().Log;
        var clientLog = new ConcurrentQueue<string>();

        void ClientLog(string value)
        {
            Trace($"client log: {value}");
            clientLog.Enqueue(value);
        }

        var cts = new CancellationTokenSource();

        // act
        var o1 = await client.Demo.SubscribeFirstAsync(new FirstSubscriptionInit { Param = "abc" }, cts.Token).GetData();
        var os1 = o1.Subscribe(ClientLog);
        Trace("first subscribed");
        var o2 = await client.Demo.SubscribeSecondAsync(new SecondSubscriptionInit { Param = "def" }, cts.Token).GetData();
        var os2 = o2.Subscribe(ClientLog);
        Trace("second subscribed");
        // wait for init and msg entries
        Trace("wait for init and msg log entries");
        await Wait.UntilAsync(() => serverLog.Count == 6 && clientLog.Count == 4);

        Trace("dispose subscriptions");
        cts.Cancel();
        os1.Dispose();
        os2.Dispose();
        Trace("await subscription 1");
        await o1.WhenCompleted();
        Trace("await subscription 2");
        await o2.WhenCompleted();

        // wait for cancellation entries
        Trace("wait for cancellation log entries");
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

        Trace("done");

        void Trace(string value)
        {
            Console.WriteLine($"{nameof(DebugSubscription_Works)} - {value}");
        }
    }
}