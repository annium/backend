using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Client;

namespace Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient.Clients
{
    public class DemoClient
    {
        private readonly IClientBase _client;

        public DemoClient(IClientBase client)
        {
            _client = client;
        }

        public Task<IStatusResult<OperationStatus, string>> EchoAsync(EchoRequest request) =>
            _client.FetchAsync<string>(request);

        public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeFirstAsync(FirstSubscriptionInit init, CancellationToken ct) =>
            _client.SubscribeAsync<FirstSubscriptionInit, string>(init, ct);

        public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeSecondAsync(SecondSubscriptionInit init, CancellationToken ct) =>
            _client.SubscribeAsync<SecondSubscriptionInit, string>(init, ct);
    }
}