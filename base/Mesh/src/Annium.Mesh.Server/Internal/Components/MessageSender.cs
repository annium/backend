using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Components;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal.Components;

/// <summary>
/// Implementation of message sender that serializes and sends messages over mesh connections.
/// </summary>
internal class MessageSender : IMessageSender, ILogSubject
{
    /// <summary>
    /// Gets the logger for this message sender.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The serializer used to serialize messages and message data.
    /// </summary>
    private readonly ISerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSender"/> class.
    /// </summary>
    /// <param name="serializer">The serializer for message data.</param>
    /// <param name="logger">The logger for this message sender.</param>
    public MessageSender(ISerializer serializer, ILogger logger)
    {
        Logger = logger;
        _serializer = serializer;
    }

    /// <summary>
    /// Sends a message asynchronously with an auto-generated message ID.
    /// </summary>
    /// <typeparam name="T">The type of the message data.</typeparam>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="action">The action identifier.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<ConnectionSendStatus> SendAsync<T>(
        Guid cid,
        ISendingConnection cn,
        ushort version,
        MessageType messageType,
        int action,
        T data,
        CancellationToken ct = default
    )
        where T : notnull
    {
        return SendAsync(cid, cn, Guid.NewGuid(), version, messageType, action, data, ct);
    }

    /// <summary>
    /// Sends a message asynchronously with a specific message ID.
    /// </summary>
    /// <typeparam name="T">The type of the message data.</typeparam>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="action">The action identifier.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<ConnectionSendStatus> SendAsync<T>(
        Guid cid,
        ISendingConnection cn,
        Guid id,
        ushort version,
        MessageType messageType,
        int action,
        T data,
        CancellationToken ct = default
    )
        where T : notnull
    {
        return SendAsync(cid, cn, id, version, messageType, action, typeof(T), data, ct);
    }

    /// <summary>
    /// Sends a message asynchronously with dynamic type information and an auto-generated message ID.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="action">The action identifier.</param>
    /// <param name="dataType">The type of the message data.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<ConnectionSendStatus> SendAsync(
        Guid cid,
        ISendingConnection cn,
        ushort version,
        MessageType messageType,
        int action,
        Type dataType,
        object data,
        CancellationToken ct = default
    )
    {
        return SendAsync(cid, cn, Guid.NewGuid(), version, messageType, action, dataType, data, ct);
    }

    /// <summary>
    /// Sends a message asynchronously with dynamic type information and a specific message ID.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="action">The action identifier.</param>
    /// <param name="dataType">The type of the message data.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    public async ValueTask<ConnectionSendStatus> SendAsync(
        Guid cid,
        ISendingConnection cn,
        Guid id,
        ushort version,
        MessageType messageType,
        int action,
        Type dataType,
        object data,
        CancellationToken ct = default
    )
    {
        this.Trace("cn {cid}: serialize {id} {data}", cid, id, data);
        var message = new Message
        {
            Id = id,
            Version = version,
            Type = messageType,
            Action = action,
            Data = _serializer.SerializeData(dataType, data),
        };
        var raw = _serializer.SerializeMessage(message);

        this.Trace("cn {cid}: send {id} {data}", cid, id, data);
        var status = await cn.SendAsync(raw, ct).ConfigureAwait(false);

        this.Trace("cn {cid}: sent {id} {data} with {status}", cid, id, data, status);

        return status;
    }
}

/// <summary>
/// Extension methods for <see cref="IMessageSender"/> that provide overloads accepting <see cref="ActionKey"/> parameters.
/// </summary>
internal static class MessageSenderExtensions
{
    /// <summary>
    /// Sends a message asynchronously using an action key with an auto-generated message ID.
    /// </summary>
    /// <typeparam name="T">The type of the message data.</typeparam>
    /// <param name="sender">The message sender.</param>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="key">The action key containing version and action information.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<ConnectionSendStatus> SendAsync<T>(
        this IMessageSender sender,
        Guid cid,
        ISendingConnection cn,
        ActionKey key,
        MessageType messageType,
        T data,
        CancellationToken ct = default
    )
        where T : notnull
    {
        return sender.SendAsync(cid, cn, Guid.NewGuid(), key, messageType, data, ct);
    }

    /// <summary>
    /// Sends a message asynchronously using an action key with a specific message ID.
    /// </summary>
    /// <typeparam name="T">The type of the message data.</typeparam>
    /// <param name="sender">The message sender.</param>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="key">The action key containing version and action information.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<ConnectionSendStatus> SendAsync<T>(
        this IMessageSender sender,
        Guid cid,
        ISendingConnection cn,
        Guid id,
        ActionKey key,
        MessageType messageType,
        T data,
        CancellationToken ct = default
    )
        where T : notnull
    {
        return sender.SendAsync(cid, cn, id, key.Version, messageType, key.Action, data, ct);
    }

    /// <summary>
    /// Sends a message asynchronously using an action key with dynamic type information and an auto-generated message ID.
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="key">The action key containing version and action information.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="dataType">The type of the message data.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<ConnectionSendStatus> SendAsync(
        this IMessageSender sender,
        Guid cid,
        ISendingConnection cn,
        ActionKey key,
        MessageType messageType,
        Type dataType,
        object data,
        CancellationToken ct = default
    )
    {
        return sender.SendAsync(cid, cn, Guid.NewGuid(), key, messageType, dataType, data, ct);
    }

    /// <summary>
    /// Sends a message asynchronously using an action key with dynamic type information and a specific message ID.
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="id">The message identifier.</param>
    /// <param name="key">The action key containing version and action information.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="dataType">The type of the message data.</param>
    /// <param name="data">The message data.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task that represents the asynchronous send operation and returns the connection send status.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<ConnectionSendStatus> SendAsync(
        this IMessageSender sender,
        Guid cid,
        ISendingConnection cn,
        Guid id,
        ActionKey key,
        MessageType messageType,
        Type dataType,
        object data,
        CancellationToken ct = default
    )
    {
        return sender.SendAsync(cid, cn, id, key.Version, messageType, key.Action, dataType, data, ct);
    }
}
