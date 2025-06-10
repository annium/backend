using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Collections.Generic;
using Annium.Data.Operations;
using Annium.Extensions.Reactive.Operators;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

/// <summary>
/// Base implementation for mesh clients providing core communication functionality
/// </summary>
internal abstract class ClientBase : IClientBase
{
    /// <summary>
    /// Gets the logger instance for diagnostics
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The underlying connection for sending and receiving messages
    /// </summary>
    private readonly ISendingReceivingConnection _connection;

    /// <summary>
    /// The serializer for message data
    /// </summary>
    private readonly ISerializer _serializer;

    /// <summary>
    /// The client configuration settings
    /// </summary>
    private readonly IClientConfiguration _configuration;

    /// <summary>
    /// Container for disposable resources
    /// </summary>
    private readonly DisposableBox _disposable;

    /// <summary>
    /// Dictionary of pending request futures with expiration support
    /// </summary>
    private readonly ExpiringDictionary<Guid, RequestFuture> _requestFutures;

    /// <summary>
    /// Dictionary of active subscriptions
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new();

    /// <summary>
    /// Observable stream of incoming messages
    /// </summary>
    private readonly IObservable<Message> _messages;

    /// <summary>
    /// Flag indicating whether the client has been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the ClientBase class
    /// </summary>
    /// <param name="connection">The connection for sending and receiving messages</param>
    /// <param name="timeProvider">The time provider for timeout operations</param>
    /// <param name="serializer">The serializer for message data</param>
    /// <param name="configuration">The client configuration</param>
    /// <param name="logger">The logger for diagnostics</param>
    protected ClientBase(
        ISendingReceivingConnection connection,
        ITimeProvider timeProvider,
        ISerializer serializer,
        IClientConfiguration configuration,
        ILogger logger
    )
    {
        Logger = logger;
        _connection = connection;
        _serializer = serializer;
        _configuration = configuration;

        this.Trace("start");
        _disposable = Disposable.Box(logger);
        _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);
        _messages = _connection.Observe().Select(serializer.DeserializeMessage).Publish().RefCount();
        _disposable += _messages
            .Where(x => x.Type is MessageType.ConnectionReady)
            .Subscribe(_ => HandleConnectionReady());
        _disposable += _messages
            .Where(x => x.Type is MessageType.Response or MessageType.SubscriptionConfirm)
            .SubscribeOn(TaskPoolScheduler.Default)
            .Subscribe(HandleResponseMessage);
        this.Trace("done");
    }

    /// <summary>
    /// Creates an observable stream for listening to broadcast/push notifications of the specified type
    /// </summary>
    /// <typeparam name="TNotification">The type of notifications to listen for</typeparam>
    /// <returns>An observable stream of notifications</returns>
    public IObservable<TNotification> Listen<TNotification>()
    {
        return _messages
            .Where(x => x.Type is MessageType.Push)
            .Select(x =>
            {
                this.Trace<Message, string>("try parse {message} as {type}", x, typeof(TNotification).FriendlyName());
                return _serializer.DeserializeData(typeof(TNotification), x.Data);
            })!
            .OfType<TNotification>()
            .SubscribeOn(TaskPoolScheduler.Default);
    }

    // // event
    // public void Notify<TEvent>(
    //     TEvent ev
    // )
    //     where TEvent : EventBaseObsolete
    // {
    //     Task.Run(() => SendInternal(ev)).ConfigureAwait(false);
    // }

    /// <summary>
    /// Sends a request without expecting a response
    /// </summary>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status result</returns>
    public Task<IStatusResult<OperationStatus>> SendAsync(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    )
    {
        return FetchInternalAsync(version, action, request, ct);
    }

    /// <summary>
    /// Sends a request and expects a typed response
    /// </summary>
    /// <typeparam name="TData">The expected response data type</typeparam>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status and response data</returns>
    public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    )
        where TData : notnull
    {
        return FetchInternalAsync(version, action, request, default(TData)!, ct);
    }

    /// <summary>
    /// Sends a request and expects a typed response with a default value fallback
    /// </summary>
    /// <typeparam name="TData">The expected response data type</typeparam>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="defaultValue">The default value to return if the request fails</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status and response data or default value</returns>
    public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        TData defaultValue,
        CancellationToken ct = default
    )
        where TData : notnull
    {
        return FetchInternalAsync(version, action, request, defaultValue, ct);
    }

    // // init subscription
    // public async Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
    //     TInit request,
    //     CancellationToken ct = default
    // )
    //     where TInit : SubscriptionInitRequestBaseObsolete
    // {
    //     var type = typeof(TInit).FriendlyName();
    //     var subscriptionId = request.Rid;
    //
    //     this.Trace("{type}#{subId} - start, create observable", type, subscriptionId);
    //     var channel = Channel.CreateUnbounded<TMessage>();
    //     var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    //
    //     async void OnDisposed()
    //     {
    //         if (!_subscriptions.TryRemove(subscriptionId, out _))
    //         {
    //             this.Trace("{type}#{subId} - skipped disposal of untracked subscription", type, subscriptionId);
    //             return;
    //         }
    //
    //         this.Trace("{type}#{subId} - unsubscribe on server", type, subscriptionId);
    //         await FetchInternal(SubscriptionCancelRequestObsolete.New(subscriptionId), CancellationToken.None);
    //         this.Trace("{type}#{subId} - unsubscribed on server", type, subscriptionId);
    //     }
    //
    //     this.Trace("{type}#{subId} - create observable", type, subscriptionId);
    //     _messageObservable
    //         .OfType<SubscriptionMessageObsolete<TMessage>>()
    //         .Where(x => x.SubscriptionId == subscriptionId)
    //         .Select(x => x.Message)
    //         .WriteToChannel(channel.Writer, cts.Token);
    //
    //     this.Trace("{type}#{subId} - init", type, subscriptionId);
    //     var response = await FetchInternal(request, Guid.Empty, ct);
    //     if (response.HasErrors)
    //     {
    //         this.Trace("{type}#{subId} - failed: {response}", type, subscriptionId, response);
    //         cts.Cancel();
    //         return Result.Status(response.Status, Observable.Empty<TMessage>()).Join(response);
    //     }
    //
    //     this.Trace("{type}#{subId} - subscribe", type, subscriptionId);
    //     var observable = ObservableExt.FromChannel(channel.Reader, OnDisposed)
    //         .ObserveOn(TaskPoolScheduler.Default);
    //
    //     this.Trace("{type}#{subId} - track observable", type, subscriptionId);
    //     if (!_subscriptions.TryAdd(subscriptionId, new(cts, (IObservable<object>)observable)))
    //         throw new InvalidOperationException($"Subscription {subscriptionId} is already tracked");
    //
    //     return Result.Status(response.Status, observable);
    // }

    /// <summary>
    /// Asynchronously disposes the client and its resources
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        if (_isDisposed)
        {
            this.Trace("already disposed");
            return;
        }

        this.Trace("start, dispose subscriptions");
        await Task.WhenAll(
            _subscriptions.Values.Select(async x =>
            {
                await x.Cts.CancelAsync();
                await x.Observable.WhenCompletedAsync(Logger);
            })
        );

        this.Trace("dispose disposable box");
        await _disposable.DisposeAsync();

        this.Trace("handle disposal in inherited class");
        HandleDispose();

        _isDisposed = true;

        this.Trace("done");
    }

    /// <summary>
    /// Handles disposal of client-specific resources in derived classes
    /// </summary>
    protected abstract void HandleDispose();

    /// <summary>
    /// Handles the connection ready event in derived classes
    /// </summary>
    protected abstract void HandleConnectionReady();

    /// <summary>
    /// Internal method for sending requests without expecting a typed response
    /// </summary>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status result</returns>
    private async Task<IStatusResult<OperationStatus>> FetchInternalAsync(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct
    )
    {
        var (result, response) = await FetchRawAsync<IStatusResult<OperationStatus>>(version, action, request, ct);

        return response ?? result;
    }

    /// <summary>
    /// Internal method for sending requests and expecting a typed response
    /// </summary>
    /// <typeparam name="TData">The expected response data type</typeparam>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="defaultValue">The default value to return if the request fails</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status and response data or default value</returns>
    private async Task<IStatusResult<OperationStatus, TData?>> FetchInternalAsync<TData>(
        ushort version,
        Enum action,
        object request,
        TData defaultValue,
        CancellationToken ct
    )
        where TData : notnull
    {
        var (result, response) = await FetchRawAsync<IStatusResult<OperationStatus, TData?>>(
            version,
            action,
            request,
            ct
        );

        return response ?? Result.Status<OperationStatus, TData?>(result.Status, defaultValue).Join(result);
    }

    /// <summary>
    /// Internal method for sending raw requests and handling the response
    /// </summary>
    /// <typeparam name="TResponse">The expected response type</typeparam>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status result and response data</returns>
    private async Task<(IStatusResult<OperationStatus>, TResponse?)> FetchRawAsync<TResponse>(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct
    )
        where TResponse : notnull
    {
        var id = Guid.NewGuid();
        var tcs = new TaskCompletionSource<object>();
        using var cts = new CancellationTokenSource(_configuration.ResponseTimeout.ToTimeSpan());
        // external token - operation canceled
        ct.Register(() =>
        {
            // if not arrived and not expired - cancel
            if (!tcs.Task.IsCompleted && !cts.IsCancellationRequested)
            {
                this.Trace("request {id} - cancel operation", id);
                cts.Cancel();
                tcs.TrySetException(new OperationCanceledException(ct));
            }
        });
        cts.Token.Register(() =>
        {
            // if not arrived and not canceled - expire
            if (!tcs.Task.IsCompleted && !ct.IsCancellationRequested)
            {
                this.Trace("request {id} - cancel by timeout", id);
                tcs.TrySetException(new TimeoutException());
            }
        });

        _requestFutures.Add(id, new RequestFuture(tcs, cts, typeof(TResponse)), _configuration.ResponseTimeout);

        try
        {
            var data = _serializer.SerializeData(request.GetType(), request);
            var message = new Message
            {
                Id = id,
                Version = version,
                Type = MessageType.Request,
                Action = Convert.ToInt32(action),
                Data = data,
            };
            if (!await SendInternalAsync(message))
                return (Result.Status(OperationStatus.NetworkError).Error("Socket is closed"), default);

            var response = (TResponse)await tcs.Task;
            return (Result.Status(OperationStatus.Ok), response);
        }
        catch (OperationCanceledException)
        {
            return (Result.Status(OperationStatus.Aborted), default);
        }
        catch (TimeoutException)
        {
            return (Result.Status(OperationStatus.Timeout).Error("Operation timed out"), default);
        }
        catch (Exception e)
        {
            return (Result.Status(OperationStatus.UncaughtError).Error(e.Message), default);
        }
    }

    /// <summary>
    /// Internal method for sending a message through the connection
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <returns>A task containing a value indicating whether the send operation was successful</returns>
    private async Task<bool> SendInternalAsync(Message message)
    {
        this.Trace("send message {message}", message);
        var result = await _connection.SendAsync(_serializer.SerializeMessage(message), CancellationToken.None);

        return result is ConnectionSendStatus.Ok;
    }

    /// <summary>
    /// Handles incoming response messages from the server
    /// </summary>
    /// <param name="message">The response message to handle</param>
    private void HandleResponseMessage(Message message)
    {
        if (!_requestFutures.Remove(message.Id, out var future))
            return;

        if (future.CancellationSource.IsCancellationRequested)
        {
            this.Trace("dismiss message {message} - request was cancelled", message);
            return;
        }

        var response = ParseResponseMessage(message, future.ResponseType);
        if (response is null)
        {
            this.Error("failed to parse response from message {message}", message);
            return;
        }

        this.Trace("complete future with response from message {message}", message);
        future.TaskSource.TrySetResult(response);
    }

    /// <summary>
    /// Parses a response message into the expected response type
    /// </summary>
    /// <param name="message">The message to parse</param>
    /// <param name="responseType">The expected response type</param>
    /// <returns>The parsed response object or null if parsing failed</returns>
    private object? ParseResponseMessage(Message message, Type? responseType)
    {
        this.Trace("parse message {message} to response", message);

        if (responseType is null)
        {
            this.Trace(
                "parsing message {message} to data response failed - no response type stored with future",
                message
            );
            return null;
        }

        var response = _serializer.DeserializeData(responseType, message.Data);
        this.Trace("parsed message {message} to response {response}", message, response);

        return response;
    }

    /// <summary>
    /// Represents a future request awaiting a response
    /// </summary>
    /// <param name="TaskSource">The task completion source for the response</param>
    /// <param name="CancellationSource">The cancellation token source for timeout handling</param>
    /// <param name="ResponseType">The expected response type</param>
    private record struct RequestFuture(
        TaskCompletionSource<object> TaskSource,
        CancellationTokenSource CancellationSource,
        Type? ResponseType
    );

    /// <summary>
    /// Represents an active subscription with cancellation support
    /// </summary>
    /// <param name="Cts">The cancellation token source for the subscription</param>
    /// <param name="Observable">The observable stream for the subscription</param>
    private record struct Subscription(CancellationTokenSource Cts, IObservable<object> Observable);
}
