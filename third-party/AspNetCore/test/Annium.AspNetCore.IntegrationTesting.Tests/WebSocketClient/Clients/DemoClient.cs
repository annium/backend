using System;
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

        public Task<IStatusResult<OperationStatus, string>> EchoAsync(EchoRequest request) =>
            _client.FetchAsync<string>(request);

        public IObservable<string> ListenFirst(FirstSubscriptionInit? init = default) =>
            _client.Listen<FirstSubscriptionInit, string>(init ?? new FirstSubscriptionInit());

        public IObservable<string> ListenSecond(SecondSubscriptionInit? init = default) =>
            _client.Listen<SecondSubscriptionInit, string>(init ?? new SecondSubscriptionInit());
    }
}