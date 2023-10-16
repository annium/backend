using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Client;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Client.Clients;

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
    ) => _client.FetchAsync(request, defaultValue, ct);

    public void Analytics(
        AnalyticEvent e
    ) => _client.Notify(e);

    public IObservable<CounterMessage> ListenCounter() => _client.Listen<CounterMessage>();


    public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeFirstAsync(
        FirstSubscriptionInit init,
        CancellationToken ct = default
    ) => _client.SubscribeAsync<FirstSubscriptionInit, string>(init, ct);

    public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeFirstAsync(
        CancellationToken ct = default
    ) => _client.SubscribeAsync<FirstSubscriptionInit, string>(new FirstSubscriptionInit(), ct);

    public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeSecondAsync(
        SecondSubscriptionInit init,
        CancellationToken ct = default
    ) => _client.SubscribeAsync<SecondSubscriptionInit, string>(init, ct);

    public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeSecondAsync(
        CancellationToken ct = default
    ) => _client.SubscribeAsync<SecondSubscriptionInit, string>(new SecondSubscriptionInit(), ct);
}