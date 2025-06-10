using System;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents a connection that can receive messages
/// </summary>
public interface IReceivingConnection
{
    /// <summary>
    /// Event is invoked, when message is received.
    /// Message must be processed synchronously due to possible buffer overwriting in implementing transports
    /// </summary>
    event Action<ReadOnlyMemory<byte>> OnReceived;
}
