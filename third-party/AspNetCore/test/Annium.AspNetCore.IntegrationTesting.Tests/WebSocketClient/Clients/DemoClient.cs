using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Client;

namespace Annium.AspNetCore.IntegrationTesting.WebSocketClient.Clients
{
    public class DemoClient
    {
        private readonly IClientBase _client;

        public DemoClient(IClientBase client)
        {
            _client = client;
        }

        public Task<IStatusResult<OperationStatus, string>> EchoAsync(
            EchoRequest request,
            CancellationToken ct = default
        ) => _client.FetchAsync<string>(request, ct);

        public Task<IStatusResult<OperationStatus, string>> EchoAsync(
            EchoRequest request,
            string defaultValue,
            CancellationToken ct = default
        ) => _client.FetchAsync<string>(request, defaultValue, ct);


        public Task<IStatusResult<OperationStatus, IAsyncDisposableObservable<string>>> SubscribeFirstAsync(
            FirstSubscriptionInit init,
            CancellationToken ct = default
        ) => _client.SubscribeAsync<FirstSubscriptionInit, string>(init, ct);

        public Task<IStatusResult<OperationStatus, IAsyncDisposableObservable<string>>> SubscribeFirstAsync(
            CancellationToken ct = default
        ) => _client.SubscribeAsync<FirstSubscriptionInit, string>(new FirstSubscriptionInit(), ct);

        public Task<IStatusResult<OperationStatus, IAsyncDisposableObservable<string>>> SubscribeSecondAsync(
            SecondSubscriptionInit init,
            CancellationToken ct = default
        ) => _client.SubscribeAsync<SecondSubscriptionInit, string>(init, ct);

        public Task<IStatusResult<OperationStatus, IAsyncDisposableObservable<string>>> SubscribeSecondAsync(
            CancellationToken ct = default
        ) => _client.SubscribeAsync<SecondSubscriptionInit, string>(new SecondSubscriptionInit(), ct);
    }
}