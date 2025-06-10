using System;

namespace Annium.Mesh.Server.Internal.Models;

/// <summary>
/// Provides factory methods for creating push messages.
/// </summary>
internal static class PushMessage
{
    /// <summary>
    /// Creates a new push message with the specified connection ID and message.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <param name="connectionId">The identifier of the connection to send the message to.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>A new push message instance.</returns>
    public static PushMessage<T> New<T>(Guid connectionId, T message) => new(connectionId, message);
}

/// <summary>
/// Represents a message that is pushed to a specific connection.
/// </summary>
/// <typeparam name="T">The type of the message.</typeparam>
/// <param name="ConnectionId">The identifier of the connection to send the message to.</param>
/// <param name="Message">The message to send.</param>
internal sealed record PushMessage<T>(Guid ConnectionId, T Message);
