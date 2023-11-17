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

internal class MessageSender : IMessageSender, ILogSubject
{
    public ILogger Logger { get; }
    private readonly ISerializer _serializer;

    public MessageSender(ISerializer serializer, ILogger logger)
    {
        Logger = logger;
        _serializer = serializer;
    }

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
            Data = _serializer.SerializeData(dataType, data)
        };
        var raw = _serializer.SerializeMessage(message);

        this.Trace("cn {cid}: send {id} {data}", cid, id, data);
        var status = await cn.SendAsync(raw, ct).ConfigureAwait(false);

        this.Trace("cn {cid}: sent {id} {data} with {status}", cid, id, data, status);

        return status;
    }
}

internal static class MessageSenderExtensions
{
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
