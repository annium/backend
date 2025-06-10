using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Client;
using Annium.Mesh.Tests.System.Domain;
using Action = Annium.Mesh.Tests.System.Domain.Action;

namespace Annium.Mesh.Tests.System.Client.Clients;

/// <summary>
/// Demo client providing typed methods for interacting with demo mesh server functionality.
/// </summary>
public class DemoClient
{
    /// <summary>
    /// The underlying mesh client base for server communication.
    /// </summary>
    private readonly IClientBase _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoClient"/> class with the specified client base.
    /// </summary>
    /// <param name="client">The underlying mesh client base instance.</param>
    public DemoClient(IClientBase client)
    {
        _client = client;
    }

    /// <summary>
    /// Sends an echo request to the server and returns the echoed message.
    /// </summary>
    /// <param name="request">The echo request containing the message to echo.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation with the server response.</returns>
    public Task<IStatusResult<OperationStatus, string?>> EchoAsync(
        EchoRequest request,
        CancellationToken ct = default
    ) => _client.FetchAsync<string>(1, Action.Echo, request, ct);

    /// <summary>
    /// Sends an echo request to the server and returns the echoed message with a default fallback value.
    /// </summary>
    /// <param name="request">The echo request containing the message to echo.</param>
    /// <param name="defaultValue">The default value to return if the operation fails.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation with the server response.</returns>
    public Task<IStatusResult<OperationStatus, string?>> EchoAsync(
        EchoRequest request,
        string defaultValue,
        CancellationToken ct = default
    ) => _client.FetchAsync(1, Action.Echo, request, defaultValue, ct);

    // public void Analytics(
    //     AnalyticEvent e
    // ) => _client.Notify(e);

    /// <summary>
    /// Creates an observable stream for listening to counter messages from the server.
    /// </summary>
    /// <returns>An observable stream of counter messages.</returns>
    public IObservable<CounterMessage> ListenCounter() => _client.Listen<CounterMessage>();

    // public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeFirstAsync(
    //     FirstSubscriptionInit init,
    //     CancellationToken ct = default
    // ) => _client.SubscribeAsync<FirstSubscriptionInit, string>(init, ct);
    //
    // public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeFirstAsync(
    //     CancellationToken ct = default
    // ) => _client.SubscribeAsync<FirstSubscriptionInit, string>(new FirstSubscriptionInit(), ct);
    //
    // public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeSecondAsync(
    //     SecondSubscriptionInit init,
    //     CancellationToken ct = default
    // ) => _client.SubscribeAsync<SecondSubscriptionInit, string>(init, ct);
    //
    // public Task<IStatusResult<OperationStatus, IObservable<string>>> SubscribeSecondAsync(
    //     CancellationToken ct = default
    // ) => _client.SubscribeAsync<SecondSubscriptionInit, string>(new SecondSubscriptionInit(), ct);
}
