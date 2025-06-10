namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents the status of a connection closure
/// </summary>
public enum ConnectionCloseStatus
{
    /// <summary>
    /// Connection was closed by the local side
    /// </summary>
    ClosedLocal,

    /// <summary>
    /// Connection was closed by the remote side
    /// </summary>
    ClosedRemote,

    /// <summary>
    /// Connection was closed due to an error
    /// </summary>
    Error,
}
