using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient.Clients;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;
using Xunit.Abstractions;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public class WebSocketDebugTest : IntegrationTestBase
{
    public WebSocketDebugTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    private Task<TestServerTestClient> GetClient() => AppFactory.GetWebSocketClientAsync<TestServerTestClient>("/ws");

    [Fact]
    public async Task DebugRequestResponse_Works()
    {
        this.Trace("start");

        // arrange
        var message = "test message";
        this.Trace("get client");
        await using var client = await GetClient();

        // act
        this.Trace("send request");
        var response = await client.Demo.EchoAsync(new EchoRequest(message));

        // assert
        this.Trace("validate response");
        response.Status.Is(OperationStatus.Ok);
        response.Data.Is(message);

        this.Trace("done");
    }

    [Fact]
    public async Task DebugSubscription_Works()
    {
        this.Trace("start");

        // arrange
        this.Trace("get client");
        await using var client = await GetClient();

        var serverLog = AppFactory.Resolve<SharedDataContainer>().Log;
        var clientLog = new ConcurrentQueue<string>();

        void ClientLog(string value)
        {
            this.Trace<string>("client log: {value}", value);
            clientLog.Enqueue(value);
        }

        var cts = new CancellationTokenSource();

        // act
        this.Trace("subscribe first");
        var o1 = await client.Demo.SubscribeFirstAsync(new FirstSubscriptionInit { Param = "abc" }, cts.Token).GetData();

        this.Trace("schedule first completion tracking");
        var os1 = o1.Subscribe(ClientLog);
        this.Trace("first subscribed");

        this.Trace("subscribe second");
        var o2 = await client.Demo.SubscribeSecondAsync(new SecondSubscriptionInit { Param = "def" }, cts.Token).GetData();

        this.Trace("schedule second completion tracking");
        var os2 = o2.Subscribe(ClientLog);
        this.Trace("second subscribed");

        // wait for init and msg entries
        this.Trace("wait for init and msg log entries");
        await Expect.To(() =>
        {
            this.Trace("assert client/server log");
            serverLog.Has(6);
            clientLog.Has(4);
        }, 2000);

        this.Trace("dispose subscriptions");
        cts.Cancel();
        os1.Dispose();
        os2.Dispose();

        // wait for cancellation entries
        this.Trace("wait for cancellation log entries");
        await Expect.To(() => serverLog.Has(8), 2000);

        // assert
        this.Trace("verify log");
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

        this.Trace("done");
    }
}