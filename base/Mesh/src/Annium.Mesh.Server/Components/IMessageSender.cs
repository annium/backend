using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Domain;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Components;

public interface IMessageSender
{
    ValueTask<ConnectionSendStatus> SendAsync<T>(
        Guid cid,
        ISendingConnection cn,
        ushort version,
        MessageType messageType,
        int action,
        T data,
        CancellationToken ct = default
    )
        where T : notnull;
    ValueTask<ConnectionSendStatus> SendAsync<T>(
        Guid cid,
        ISendingConnection cn,
        Guid id,
        ushort version,
        MessageType messageType,
        int action,
        T data,
        CancellationToken ct = default
    )
        where T : notnull;
    ValueTask<ConnectionSendStatus> SendAsync(
        Guid cid,
        ISendingConnection cn,
        ushort version,
        MessageType messageType,
        int action,
        Type dataType,
        object data,
        CancellationToken ct = default
    );
    ValueTask<ConnectionSendStatus> SendAsync(
        Guid cid,
        ISendingConnection cn,
        Guid id,
        ushort version,
        MessageType messageType,
        int action,
        Type dataType,
        object data,
        CancellationToken ct = default
    );
}
