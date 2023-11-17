using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Server.Components;
using Annium.Mesh.Server.Internal.Components;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal.Models;

internal class PushContext<TMessage> : IPushContext<TMessage>, IAsyncDisposable, ILogSubject
    where TMessage : notnull
{
    public ILogger Logger { get; }
    private readonly IMessageSender _sender;
    private readonly ActionKey _actionKey;
    private readonly Guid _cid;
    private readonly ISendingConnection _cn;
    private readonly CancellationToken _ct;
    private readonly IExecutor _executor;

    public PushContext(
        IMessageSender sender,
        ActionKey actionKey,
        Guid cid,
        ISendingConnection cn,
        CancellationToken ct,
        ILogger logger
    )
    {
        Logger = logger;
        _sender = sender;
        _actionKey = actionKey;
        _cid = cid;
        _cn = cn;
        _ct = ct;
        _executor = Executor.Sequential<PushContext<TMessage>>(logger);
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

    private void SendInternal(TMessage msg)
    {
        this.Trace("cn {id}: schedule send of {message}", _cid, msg);
        _executor.Schedule(
            async () => await _sender.SendAsync(_cid, _cn, _actionKey, MessageType.Push, msg, _ct).ConfigureAwait(false)
        );
    }
}
