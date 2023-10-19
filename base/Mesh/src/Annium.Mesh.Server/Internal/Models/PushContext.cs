using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal.Models;

internal class PushContext<TMessage> : IPushContext<TMessage>, IAsyncDisposable, ILogSubject
{
    public ILogger Logger { get; }
    private readonly ActionKey _actionKey;
    private readonly ISerializer _serializer;
    private readonly Guid _cid;
    private readonly ISendingConnection _cn;
    private readonly CancellationToken _ct;
    private readonly IBackgroundExecutor _executor;

    public PushContext(
        ActionKey actionKey,
        ISerializer serializer,
        Guid cid,
        ISendingConnection cn,
        CancellationToken ct,
        ILogger logger
    )
    {
        Logger = logger;
        _actionKey = actionKey;
        _serializer = serializer;
        _cid = cid;
        _cn = cn;
        _ct = ct;
        _executor = Executor.Background.Sequential<PushContext<TMessage>>(logger);
        _executor.Start();
    }


    public void Send(TMessage message)
    {
        if (_ct.IsCancellationRequested)
        {
            this.Trace("cn {id}: skip send of {message} - cancellation is requested", _cid, message);
            return;
        }

        SendInternal(message);
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("connection {id} - start", _cid);
        await _executor.DisposeAsync();
        this.Trace("connection {id} - done", _cid);
    }

    private void SendInternal<T>(T msg)
    {
        this.Trace("cn {id}: schedule send of {message}", _cid, msg);
        _executor.Schedule(async () =>
        {
            this.Trace("cn {id}: serialize {message}", _cid, msg);
            var data = _serializer.SerializeData(msg);
            var message = new Message
            {
                Version = _actionKey.Version,
                Type = MessageType.Push,
                Action = _actionKey.Action,
                Data = data
            };
            var push = _serializer.SerializeMessage(message);

            this.Trace("cn {id}: send {message}", _cid, message);
            var status = await _cn.SendAsync(push, _ct);

            this.Trace("cn {id}: sent {message} with {status}", _cid, message, status);
        });
    }
}