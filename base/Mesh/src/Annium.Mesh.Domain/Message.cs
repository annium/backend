using System;

namespace Annium.Mesh.Domain;

/// <summary>
/// Represents a message in the mesh communication system containing metadata and binary data payload.
/// </summary>
public sealed class Message
{
    /// <summary>
    /// Gets or sets the unique identifier for this message.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the protocol version of this message.
    /// </summary>
    public ushort Version { get; init; }

    /// <summary>
    /// Gets or sets the type of message indicating its purpose and routing behavior.
    /// </summary>
    public MessageType Type { get; init; }

    /// <summary>
    /// Gets or sets the action identifier for this message, used for routing to specific handlers.
    /// </summary>
    public int Action { get; init; }

    /// <summary>
    /// Gets or sets the binary data payload of this message.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Returns a string representation of the message containing version, type, action, and data length.
    /// </summary>
    /// <returns>A formatted string describing the message.</returns>
    public override string ToString() => $"v{Version}; {Type}.{Action} | {Data.Length}";
}
