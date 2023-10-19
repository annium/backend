using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class ConnectionHandler : IAsyncDisposable, ILogSubject
{
    public ILogger Logger { get; }
    private readonly Guid _cid;
    private readonly ISendingReceivingConnection _cn;
    private readonly CancellationTokenSource _cts;
    private readonly TaskCompletionSource _tcs = new();
    private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
    private readonly LifeCycleCoordinator _lifeCycleCoordinator;
    private readonly MessageHandler _messageHandler;
    private readonly PushCoordinator _pushCoordinator;
    private readonly ISerializer _serializer;
    private readonly IBackgroundExecutor _executor;

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
        _cid = ctx.ConnectionId;
        _cn = ctx.Connection;
        _cts = ctx.Cts;
        _connectionBoundStores = connectionBoundStores;
        _lifeCycleCoordinator = lifeCycleCoordinator;
        _messageHandler = messageHandler;
        _pushCoordinator = pushCoordinator;
        _serializer = serializer;

        _executor = Executor.Background.Parallel<ConnectionHandler>(Logger);
    }

    public async Task HandleAsync()
    {
        this.Trace("cn {id} - start", _cid);

        try
        {
            // immediately subscribe to cancellation
            _cts.Token.Register(HandleConnectionCancellation);

            // start listening to messages and adding them to scheduler
            this.Trace("cn {id} - init subscription", _cid);
            _cn.Observe().Subscribe(OnMessage, OnError, OnCompleted, _cts.Token);

            // execute start hook
            this.Trace("cn {id} - handle lifecycle start", _cid);
            await _lifeCycleCoordinator.StartAsync();

            // notify client, that connection is ready
            this.Trace("cn {id} - notify connection ready", _cid);
            await _cn.SendAsync(_serializer.SerializeMessage(new Message { Type = MessageType.ConnectionReady }), _cts.Token);

            // execute run hook
            this.Trace("cn {id} - start push handlers", _cid);
            var pushTask = _pushCoordinator.RunAsync(_cid, _cn, _cts.Token);

            // start scheduler to process backlog and run upcoming work immediately
            this.Trace("cn {id} - start executor", _cid);
            _executor.Start(_cts.Token);

            // wait until connection complete
            this.Trace("cn {id} - wait until connection complete (handlers & pushers)", _cid);
            await Task.WhenAll(_tcs.Task, pushTask);

            this.Trace("cn {id} - cleanup connection-bound stores", _cid);
            await Task.WhenAll(_connectionBoundStores.Select(x => x.Cleanup(_cid)));
        }
        catch (Exception e)
        {
            this.Error(e);
        }
    }

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

    private void OnMessage(ReadOnlyMemory<byte> raw)
    {
        this.Trace("cn {id} - start", _cid);

        var message = ParseMessage(raw);
        if (message is null)
            return;

        this.Trace("cn {id} - schedule {msg}", _cid, message);
        if (_executor.TrySchedule(HandleMessage(message)))
            this.Trace("cn {id} - scheduled {msg}", _cid, message);
        else
            this.Trace("cn {id} - skipped {msg} (connection is canceled", _cid, message);

        this.Trace("cn {id} - start", _cid);
    }

    private void OnError(Exception exception)
    {
        this.Trace("cn {id} - start", _cid);

        this.Trace("cn {id} - cancel cts", _cid);
        _cts.Cancel();

        this.Error(exception);

        this.Trace("cn {id} - complete tcs due to error", _cid);
        _tcs.TrySetResult();

        this.Trace("cn {id} - done", _cid);
    }

    private void OnCompleted()
    {
        this.Trace("cn {id} - start", _cid);

        this.Trace("cn {id} - complete tcs due to connection closed", _cid);
        _tcs.TrySetResult();

        this.Trace("cn {id} - done", _cid);
    }

    private void HandleConnectionCancellation()
    {
        this.Trace("cn {id} - start", _cid);

        _tcs.TrySetResult();

        this.Trace("cn {id} - done", _cid);
    }

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

    private Func<ValueTask> HandleMessage(Message message) => async () =>
    {
        this.Trace("cn {id} - start {msg}", _cid, message);
        await _messageHandler.HandleMessage(_cn, message, _cts.Token);
        this.Trace("cn {id} - done {msg}", _cid, message);
    };
}