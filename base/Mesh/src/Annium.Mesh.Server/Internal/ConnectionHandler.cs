using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class ConnectionHandler : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
    private readonly Guid _cid;
    private readonly IServerConnection _cn;
    private readonly ISerializer _serializer;

    public ConnectionHandler(
        IServiceProvider sp,
        IEnumerable<IConnectionBoundStore> connectionBoundStores,
        Guid cid,
        IServerConnection cn,
        ISerializer serializer,
        ILogger logger
    )
    {
        Logger = logger;
        _sp = sp;
        _connectionBoundStores = connectionBoundStores;
        _cid = cid;
        _cn = cn;
        _serializer = serializer;
    }

    public async Task HandleAsync(CancellationToken ct)
    {
        this.Trace("cn {id} - start", _cid);
        await using var scope = _sp.CreateAsyncScope();
        var executor = Executor.Background.Parallel<ConnectionHandler>(Logger);
        var lifeCycleCoordinator = scope.ServiceProvider.Resolve<LifeCycleCoordinator>();
        var pusherCoordinator = scope.ServiceProvider.Resolve<PusherCoordinator>();
        try
        {
            var tcs = new TaskCompletionSource();
            tcs.Task.ContinueWith(_ => this.Trace("cn {id} - listen ended"), CancellationToken.None).GetAwaiter();

            // immediately subscribe to cancellation
            ct.Register(() =>
            {
                this.Trace("cn {id} - complete tcs due to cancellation", _cid);
                tcs.TrySetResult();
            });

            // use derived cts for connection subscription
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // start listening to messages and adding them to scheduler
            this.Trace("cn {id} - init subscription", _cid);
            _cn.Observe().Subscribe(
                HandleMessage,
                e =>
                {
                    this.Trace("cn {id} - handle error", _cid);
                    cts.Cancel();
                    this.Error(e);
                    this.Trace("cn {id} - complete tcs due to error", _cid);
                    tcs.TrySetResult();
                },
                () =>
                {
                    this.Trace("cn {id} - complete tcs due to connection closed", _cid);
                    tcs.TrySetResult();
                },
                cts.Token
            );

            // execute start hook
            this.Trace("cn {id} - handle lifecycle start - start", _cid);
            await lifeCycleCoordinator.StartAsync();
            this.Trace("cn {id} - handle lifecycle start - done", _cid);

            // notify client, that connection is ready
            this.Trace("cn {id} - notify connection ready", _cid);
            await _cn.SendAsync(_serializer.Serialize(new ConnectionReadyNotificationObsolete()), cts.Token);

            // execute run hook
            this.Trace("cn {id} - push handlers start - start", _cid);
            var pusherTask = pusherCoordinator.RunAsync(_cid, cts.Token);
            pusherTask.ContinueWith(_ => this.Trace("cn {id} - push ended"), CancellationToken.None).GetAwaiter();
            this.Trace("cn {id} - push handlers start - done", _cid);

            // start scheduler to process backlog and run upcoming work immediately
            this.Trace("cn {id} - start executor", _cid);
            executor.Start(CancellationToken.None);

            // wait until connection complete
            this.Trace("cn {id} - wait until connection complete", _cid);
            await Task.WhenAll(tcs.Task, pusherTask);
            this.Trace("cn {id} - cleanup connection-bound stores - start", _cid);
            await Task.WhenAll(_connectionBoundStores.Select(x => x.Cleanup(_cid)));
            this.Trace("cn {id} - cleanup connection-bound stores - done", _cid);
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            // all handlers must be complete before teardown lifecycle hook
            this.Trace("cn {id} - dispose executor - start", _cid);
            await executor.DisposeAsync();
            this.Trace("cn {id} - dispose executor - done", _cid);

            // execute end hook
            this.Trace("cn {id} - handle lifecycle end - start", _cid);
            await lifeCycleCoordinator.EndAsync();
            this.Trace("cn {id} - handle lifecycle end - done", _cid);
        }

        void HandleMessage(ReadOnlyMemory<byte> raw)
        {
            var request = ParseRequest(raw);
            if (request is null)
            {
                this.Warn("Failed to parse msg of size {size} bytes", raw.Length);

                return;
            }

            this.Trace("cn {id} - schedule {requestType}#{requestId}", _cid, request.Tid, request.Rid);
            executor.TrySchedule(async () =>
            {
                this.Trace("cn {id} - handle {requestType}#{requestId}", _cid, request.Tid, request.Rid);
                await using var messageScope = _sp.CreateAsyncScope();
                var handler = messageScope.ServiceProvider.Resolve<MessageHandler>();
                await handler.HandleMessage(_cn, _cid, request);
            });
        }
    }

    private AbstractRequestBaseObsolete? ParseRequest(ReadOnlyMemory<byte> msg)
    {
        try
        {
            return _serializer.Deserialize<AbstractRequestBaseObsolete>(msg);
        }
        catch (Exception e)
        {
            this.Warn(e.ToString());
            return default;
        }
    }
}