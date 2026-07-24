using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Client;
using Annium.Mesh.Transport.Abstractions;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Mesh.Tests;

/// <summary>
/// Tests for the bounded connect wait in <see cref="ClientExtensions.ConnectAsync"/>.
/// </summary>
public class ConnectTimeoutTests : Annium.Testing.TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectTimeoutTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ConnectTimeoutTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// A client that never becomes connected must not hang: ConnectAsync fails with a
    /// TimeoutException once the configured ConnectTimeout elapses, and the client is disconnected
    /// so the underlying (indefinitely retrying) transport stops trying.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ConnectAsync_NeverConnects_TimesOutAndDisconnects()
    {
        this.Trace("start");

        // arrange — a client whose Connect() never raises OnConnected
        var client = new NeverConnectingClient(Get<ILogger>(), Duration.FromMilliseconds(300));

        // act + assert — the wait is bounded; without the bound this would hang forever
        await Wrap.It(async () => await client.ConnectAsync()).ThrowsAsync<TimeoutException>();

        // assert — the failed connect stopped the retry loop
        client.DisconnectCalled.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Minimal <see cref="IClient"/> whose <see cref="Connect"/> never signals connection.
    /// </summary>
    private sealed class NeverConnectingClient : IClient
    {
        /// <summary>Gets the logger for the stub client.</summary>
        public ILogger Logger { get; }

        /// <summary>Gets the connect timeout the stub reports to the bounded connect wait.</summary>
        public Duration ConnectTimeout { get; }

        /// <summary>Gets whether <c>Disconnect</c> was called (the connect-timeout path invokes it).</summary>
        public bool DisconnectCalled { get; private set; }

        public event Action OnConnected = delegate { };
        public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
        public event Action<Exception> OnError = delegate { };

        public NeverConnectingClient(ILogger logger, Duration connectTimeout)
        {
            Logger = logger;
            ConnectTimeout = connectTimeout;
        }

        /// <summary>Never raises OnConnected — simulates a server that cannot be reached.</summary>
        public void Connect()
        {
            // intentionally never raises OnConnected — simulates a server that can't be reached
            _ = OnConnected;
            _ = OnDisconnected;
            _ = OnError;
        }

        /// <summary>Records that a disconnect was requested.</summary>
        public void Disconnect() => DisconnectCalled = true;

        /// <summary>Not supported by the stub.</summary>
        /// <typeparam name="TNotification">The notification type.</typeparam>
        /// <returns>Never returns; always throws.</returns>
        public IObservable<TNotification> Listen<TNotification>() => throw new NotSupportedException();

        /// <summary>Not supported by the stub.</summary>
        /// <param name="version">The API version.</param>
        /// <param name="action">The action.</param>
        /// <param name="request">The request payload.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Never returns; always throws.</returns>
        public Task<IStatusResult<OperationStatus>> SendAsync(
            ushort version,
            Enum action,
            object request,
            CancellationToken ct = default
        ) => throw new NotSupportedException();

        /// <summary>Not supported by the stub.</summary>
        /// <typeparam name="TData">The expected response data type.</typeparam>
        /// <param name="version">The API version.</param>
        /// <param name="action">The action.</param>
        /// <param name="request">The request payload.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Never returns; always throws.</returns>
        public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
            ushort version,
            Enum action,
            object request,
            CancellationToken ct = default
        )
            where TData : notnull => throw new NotSupportedException();

        /// <summary>Not supported by the stub.</summary>
        /// <typeparam name="TData">The expected response data type.</typeparam>
        /// <param name="version">The API version.</param>
        /// <param name="action">The action.</param>
        /// <param name="request">The request payload.</param>
        /// <param name="defaultValue">The fallback value.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Never returns; always throws.</returns>
        public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
            ushort version,
            Enum action,
            object request,
            TData defaultValue,
            CancellationToken ct = default
        )
            where TData : notnull => throw new NotSupportedException();

        /// <summary>Completes immediately; the stub owns no resources.</summary>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
