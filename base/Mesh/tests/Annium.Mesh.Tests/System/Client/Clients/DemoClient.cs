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

    /// <summary>
    /// Sends a request whose server handler always returns a non-Ok status with errors.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with the non-Ok server response.</returns>
    public Task<IStatusResult<OperationStatus, string?>> FailAsync(
        EchoRequest request,
        CancellationToken ct = default
    ) => _client.FetchAsync<string>(1, Action.Fail, request, ct);

    /// <summary>
    /// Sends a no-response-data request (exercises the SendAsync path) and returns only the status.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with the operation status.</returns>
    public Task<IStatusResult<OperationStatus>> NotifyAsync(EchoRequest request, CancellationToken ct = default) =>
        _client.SendAsync(1, Action.Notify, request, ct);

    /// <summary>
    /// Sends a request whose server handler never completes until cancelled, used to exercise caller cancellation.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with the operation result once cancelled or timed out.</returns>
    public Task<IStatusResult<OperationStatus, string?>> HangAsync(
        EchoRequest request,
        CancellationToken ct = default
    ) => _client.FetchAsync<string>(1, Action.Hang, request, ct);

    /// <summary>
    /// Sends a hang request with a default-value fallback, used to exercise the FetchAsync default-value overload
    /// and its null-response fallback branch.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="defaultValue">The value returned when no response arrives.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with the response data or the default value.</returns>
    public Task<IStatusResult<OperationStatus, string?>> HangAsync(
        EchoRequest request,
        string defaultValue,
        CancellationToken ct = default
    ) => _client.FetchAsync(1, Action.Hang, request, defaultValue, ct);

    /// <summary>
    /// Sends a request whose server handler throws, used to verify the connection survives a faulting handler.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with the operation result (no response arrives).</returns>
    public Task<IStatusResult<OperationStatus, string?>> ThrowAsync(
        EchoRequest request,
        CancellationToken ct = default
    ) => _client.FetchAsync<string>(1, Action.Throw, request, ct);

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
