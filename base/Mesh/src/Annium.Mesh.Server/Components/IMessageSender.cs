using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Domain;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Components;

/// <summary>
/// Provides methods to send messages over mesh connections with various overloads for different data types and message configurations.
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Sends a strongly-typed message over the specified connection.
    /// </summary>
    /// <typeparam name="T">The type of the message data.</typeparam>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The type of message being sent.</param>
    /// <param name="action">The action identifier for the message.</param>
    /// <param name="data">The message data to send.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the send operation with the connection send status.</returns>
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

    /// <summary>
    /// Sends a strongly-typed message with a specific message identifier over the specified connection.
    /// </summary>
    /// <typeparam name="T">The type of the message data.</typeparam>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="id">The unique message identifier.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The type of message being sent.</param>
    /// <param name="action">The action identifier for the message.</param>
    /// <param name="data">The message data to send.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the send operation with the connection send status.</returns>
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

    /// <summary>
    /// Sends a message with dynamically typed data over the specified connection.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The type of message being sent.</param>
    /// <param name="action">The action identifier for the message.</param>
    /// <param name="dataType">The type of the message data.</param>
    /// <param name="data">The message data to send.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the send operation with the connection send status.</returns>
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

    /// <summary>
    /// Sends a message with dynamically typed data and a specific message identifier over the specified connection.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection.</param>
    /// <param name="id">The unique message identifier.</param>
    /// <param name="version">The message version.</param>
    /// <param name="messageType">The type of message being sent.</param>
    /// <param name="action">The action identifier for the message.</param>
    /// <param name="dataType">The type of the message data.</param>
    /// <param name="data">The message data to send.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the send operation with the connection send status.</returns>
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
