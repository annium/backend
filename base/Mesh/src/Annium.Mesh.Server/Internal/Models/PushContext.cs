using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Server.Internal.Components;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal.Models;

/// <summary>
/// Provides a context for pushing messages to a specific client connection.
/// </summary>
/// <typeparam name="TMessage">The type of messages to push.</typeparam>
internal class PushContext<TMessage> : IPushContext<TMessage>, IAsyncDisposable, ILogSubject
    where TMessage : notnull
{
    /// <summary>
    /// Gets the logger for this push context.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The message sender used to send messages.
    /// </summary>
    private readonly IMessageSender _sender;

    /// <summary>
    /// The action key identifying the message action.
    /// </summary>
    private readonly ActionKey _actionKey;

    /// <summary>
    /// The connection identifier.
    /// </summary>
    private readonly Guid _cid;

    /// <summary>
    /// The sending connection.
    /// </summary>
    private readonly ISendingConnection _cn;

    /// <summary>
    /// The cancellation token for the operation.
    /// </summary>
    private readonly CancellationToken _ct;

    /// <summary>
    /// The executor for sequential message processing.
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PushContext{TMessage}"/> class.
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <param name="actionKey">The action key for the messages.</param>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <param name="logger">The logger for this context.</param>
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

    /// <summary>
    /// Sends a message to the connected client.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void Send(TMessage message)
    {
        if (_ct.IsCancellationRequested)
        {
            this.Trace("cn {id}: skip send of {message} - cancellation is requested", _cid, message);
            return;
        }

        SendInternal(message);
    }

    /// <summary>
    /// Asynchronously disposes the push context and its resources.
    /// </summary>
    /// <returns>A value task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("connection {id} - start", _cid);
        await _executor.DisposeAsync();
        this.Trace("connection {id} - done", _cid);
    }

    /// <summary>
    /// Internally sends a message by scheduling it for execution.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    private void SendInternal(TMessage msg)
    {
        this.Trace("cn {id}: schedule send of {message}", _cid, msg);
        _executor.Schedule(async () =>
            await _sender.SendAsync(_cid, _cn, _actionKey, MessageType.Push, msg, _ct).ConfigureAwait(false)
        );
    }
}
