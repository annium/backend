using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents a connection that can send messages
/// </summary>
public interface ISendingConnection
{
    /// <summary>
    /// Sends message
    /// </summary>
    /// <param name="data">data to be sent</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>Send status</returns>
    ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);
}
