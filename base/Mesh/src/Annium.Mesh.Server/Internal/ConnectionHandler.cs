using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Handles the lifecycle and message processing for a single client connection.
/// </summary>
internal class ConnectionHandler : IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// Gets the logger for this connection handler.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The connection context for this handler.
    /// </summary>
    private readonly ConnectionContext _ctx;

    /// <summary>
    /// The unique connection identifier.
    /// </summary>
    private readonly Guid _cid;

    /// <summary>
    /// The bidirectional connection for sending and receiving messages.
    /// </summary>
    private readonly ISendingReceivingConnection _cn;

    /// <summary>
    /// The cancellation token for this connection.
    /// </summary>
    private readonly CancellationToken _ct;

    /// <summary>
    /// Task completion source for signaling connection completion.
    /// </summary>
    private readonly TaskCompletionSource _tcs = new();

    /// <summary>
    /// Collection of connection-bound stores that need cleanup when connection ends.
    /// </summary>
    private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;

    /// <summary>
    /// Coordinator for handling connection lifecycle events.
    /// </summary>
    private readonly LifeCycleCoordinator _lifeCycleCoordinator;

    /// <summary>
    /// Handler for processing incoming messages.
    /// </summary>
    private readonly MessageHandler _messageHandler;

    /// <summary>
    /// Coordinator for handling push operations.
    /// </summary>
    private readonly PushCoordinator _pushCoordinator;

    /// <summary>
    /// Serializer for message data.
    /// </summary>
    private readonly ISerializer _serializer;

    /// <summary>
    /// Executor for parallel processing of messages.
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionHandler"/> class.
    /// </summary>
    /// <param name="ctx">The connection context.</param>
    /// <param name="connectionBoundStores">Collection of connection-bound stores.</param>
    /// <param name="lifeCycleCoordinator">Coordinator for lifecycle events.</param>
    /// <param name="messageHandler">Handler for processing messages.</param>
    /// <param name="pushCoordinator">Coordinator for push operations.</param>
    /// <param name="serializer">Serializer for message data.</param>
    /// <param name="logger">Logger for this connection handler.</param>
    public ConnectionHandler(
        ConnectionContext ctx,
        IEnumerable<IConnectionBoundStore> connectionBoundStores,
        LifeCycleCoordinator lifeCycleCoordinator,
        MessageHandler messageHandler,
        PushCoordinator pushCoordinator,
        ISerializer serializer,
        ILogger logger
    )
    {
        Logger = logger;
        _ctx = ctx;
        _cid = ctx.ConnectionId;
        _cn = ctx.Connection;
        _ct = ctx.Ct;
        _connectionBoundStores = connectionBoundStores;
        _lifeCycleCoordinator = lifeCycleCoordinator;
        _messageHandler = messageHandler;
        _pushCoordinator = pushCoordinator;
        _serializer = serializer;

        _executor = Executor.Parallel<ConnectionHandler>(Logger);
    }

    /// <summary>
    /// Handles the connection lifecycle including message processing, lifecycle events, and cleanup.
    /// </summary>
    /// <returns>A task that represents the asynchronous connection handling operation.</returns>
    public async Task HandleAsync()
    {
        this.Trace("cn {id} - start", _cid);

        try
        {
            // immediately subscribe to cancellation
            _ct.Register(HandleConnectionCancellation);

            // start listening to messages and adding them to scheduler
            this.Trace("cn {id} - init subscription", _cid);
            _cn.Observe().Subscribe(OnMessage, OnError, OnCompleted, _ct);

            // execute start hook
            this.Trace("cn {id} - handle lifecycle start", _cid);
            await _lifeCycleCoordinator.StartAsync();

            // notify client, that connection is ready
            this.Trace("cn {id} - notify connection ready", _cid);
            await _cn.SendAsync(_serializer.SerializeMessage(new Message { Type = MessageType.ConnectionReady }), _ct);

            // execute run hook
            this.Trace("cn {id} - start push handlers", _cid);
            var pushTask = _pushCoordinator.RunAsync(_cid, _cn, _ct);

            // start scheduler to process backlog and run upcoming work immediately
            this.Trace("cn {id} - start executor", _cid);
            _executor.Start(_ct);

            // wait until connection complete
            this.Trace("cn {id} - wait until connection complete (handlers & pushers)", _cid);
#pragma warning disable VSTHRD003
            await Task.WhenAll(_tcs.Task, pushTask);
#pragma warning restore VSTHRD003

            this.Trace("cn {id} - cleanup connection-bound stores", _cid);
            await Task.WhenAll(_connectionBoundStores.Select(x => x.CleanupAsync(_cid)));
        }
        catch (Exception e)
        {
            this.Error(e);
        }
    }

    /// <summary>
    /// Disposes the connection handler asynchronously, ensuring proper cleanup of resources and lifecycle events.
    /// </summary>
    /// <returns>A value task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("cn {id} - start", _cid);

        // all handlers must be complete before teardown lifecycle hook
        this.Trace("cn {id} - dispose executor", _cid);
        await _executor.DisposeAsync();

        // execute end hook
        this.Trace("cn {id} - handle lifecycle end", _cid);
        await _lifeCycleCoordinator.EndAsync();

        this.Trace("cn {id} - done", _cid);
    }

    /// <summary>
    /// Handles incoming raw message data by parsing and scheduling it for processing.
    /// </summary>
    /// <param name="raw">The raw message data received from the connection.</param>
    private void OnMessage(ReadOnlyMemory<byte> raw)
    {
        this.Trace("cn {id} - start", _cid);

        var message = ParseMessage(raw);
        if (message is null)
            return;

        this.Trace("cn {id} - schedule {msg}", _cid, message);
        if (_executor.Schedule(HandleMessage(message)))
            this.Trace("cn {id} - scheduled {msg}", _cid, message);
        else
            this.Trace("cn {id} - skipped {msg} (connection is canceled", _cid, message);

        this.Trace("cn {id} - start", _cid);
    }

    /// <summary>
    /// Handles errors that occur during message processing by canceling the connection and completing the task.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    private void OnError(Exception exception)
    {
        this.Trace("cn {id} - start", _cid);

        this.Trace("cn {id} - cancel cts", _cid);
        _ctx.Cancel();

        this.Error(exception);

        this.Trace("cn {id} - complete tcs due to error", _cid);
        _tcs.TrySetResult();

        this.Trace("cn {id} - done", _cid);
    }

    /// <summary>
    /// Handles connection completion by signaling that the connection has been closed normally.
    /// </summary>
    private void OnCompleted()
    {
        this.Trace("cn {id} - start", _cid);

        this.Trace("cn {id} - complete tcs due to connection closed", _cid);
        _tcs.TrySetResult();

        this.Trace("cn {id} - done", _cid);
    }

    /// <summary>
    /// Handles connection cancellation by completing the connection task.
    /// </summary>
    private void HandleConnectionCancellation()
    {
        this.Trace("cn {id} - start", _cid);

        _tcs.TrySetResult();

        this.Trace("cn {id} - done", _cid);
    }

    /// <summary>
    /// Parses raw message data into a Message object.
    /// </summary>
    /// <param name="raw">The raw message data to parse.</param>
    /// <returns>The parsed message, or null if parsing failed.</returns>
    private Message? ParseMessage(ReadOnlyMemory<byte> raw)
    {
        try
        {
            return _serializer.DeserializeMessage(raw);
        }
        catch (Exception e)
        {
            this.Warn("Failed to parse msg of size {size} bytes", raw.Length);
            this.Warn(e.ToString());
            return default;
        }
    }

    /// <summary>
    /// Creates a handler function for processing a parsed message.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <returns>A function that asynchronously handles the message.</returns>
    private Func<ValueTask> HandleMessage(Message message) =>
        async () =>
        {
            this.Trace("cn {id} - start {msg}", _cid, message);
            await _messageHandler.HandleMessageAsync(_cid, _cn, message, _ct);
            this.Trace("cn {id} - done {msg}", _cid, message);
        };
}
