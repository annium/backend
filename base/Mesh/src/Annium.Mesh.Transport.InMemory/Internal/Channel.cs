using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

/// <summary>
/// Represents a bi-directional communication channel between client and server connections in memory
/// </summary>
internal class Channel
{
    /// <summary>
    /// Creates a small delay for asynchronous operations
    /// </summary>
    /// <returns>A task that completes after a short delay</returns>
    private static Task DelayAsync() => Task.Delay(10);

#pragma warning disable VSTHRD110
    /// <summary>
    /// Executes an action with a delay
    /// </summary>
    /// <param name="handle">The action to execute</param>
    private static void Delay(Action handle) => DelayAsync().ContinueWith(_ => handle());
#pragma warning restore VSTHRD110

    /// <summary>
    /// Event raised when the channel is connected
    /// </summary>
    public event Action OnConnected = delegate { };

    /// <summary>
    /// Event raised when the channel is disconnected, indicating which side initiated the disconnection
    /// </summary>
    public event Action<CloseSide> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when data is received on the client side
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnClientReceived = delegate { };

    /// <summary>
    /// Event raised when data is received on the server side
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnServerReceived = delegate { };

    /// <summary>
    /// Synchronization object to ensure thread-safe access to connection state
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Indicates whether the channel is currently connected
    /// </summary>
    private bool _isConnected;

    /// <summary>
    /// Establishes the connection between client and server
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the channel is already connected</exception>
    public void Connect()
    {
        lock (_locker)
        {
            if (_isConnected)
                throw new InvalidOperationException("Channel is already connected");
            _isConnected = true;
        }

        Delay(() => OnConnected());
    }

    /// <summary>
    /// Disconnects the channel from the specified side
    /// </summary>
    /// <param name="side">The side that initiated the disconnection</param>
    public void Disconnect(CloseSide side)
    {
        lock (_locker)
        {
            if (!_isConnected)
                return;
        }

        OnDisconnected(side);
    }

    /// <summary>
    /// Sends data from client to server through the channel
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<ConnectionSendStatus> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return ValueTask.FromResult(ConnectionSendStatus.Canceled);

        lock (_locker)
            if (!_isConnected)
                return ValueTask.FromResult(ConnectionSendStatus.Closed);

        OnServerReceived(data);

        return ValueTask.FromResult(ConnectionSendStatus.Ok);
    }

    /// <summary>
    /// Sends data from server to client through the channel
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<ConnectionSendStatus> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return ValueTask.FromResult(ConnectionSendStatus.Canceled);

        lock (_locker)
            if (!_isConnected)
                return ValueTask.FromResult(ConnectionSendStatus.Closed);

        OnClientReceived(data);

        return ValueTask.FromResult(ConnectionSendStatus.Ok);
    }
}

/// <summary>
/// Specifies which side initiated the connection closure
/// </summary>
internal enum CloseSide
{
    /// <summary>
    /// The client side initiated the closure
    /// </summary>
    Client,

    /// <summary>
    /// The server side initiated the closure
    /// </summary>
    Server,
}
