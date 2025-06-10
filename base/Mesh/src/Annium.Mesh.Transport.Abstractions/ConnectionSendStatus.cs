namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents the status of a send operation on a connection
/// </summary>
public enum ConnectionSendStatus
{
    /// <summary>
    /// Message was sent successfully
    /// </summary>
    Ok,

    /// <summary>
    /// Send operation was canceled
    /// </summary>
    Canceled,

    /// <summary>
    /// Send operation failed because the connection is closed
    /// </summary>
    Closed,
}
