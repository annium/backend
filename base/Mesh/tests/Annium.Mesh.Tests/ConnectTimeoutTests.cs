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
        public ILogger Logger { get; }

        public Duration ConnectTimeout { get; }

        public bool DisconnectCalled { get; private set; }

        public event Action OnConnected = delegate { };
        public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
        public event Action<Exception> OnError = delegate { };

        public NeverConnectingClient(ILogger logger, Duration connectTimeout)
        {
            Logger = logger;
            ConnectTimeout = connectTimeout;
        }

        public void Connect()
        {
            // intentionally never raises OnConnected — simulates a server that can't be reached
            _ = OnConnected;
            _ = OnDisconnected;
            _ = OnError;
        }

        public void Disconnect() => DisconnectCalled = true;

        public IObservable<TNotification> Listen<TNotification>() => throw new NotSupportedException();

        public Task<IStatusResult<OperationStatus>> SendAsync(
            ushort version,
            Enum action,
            object request,
            CancellationToken ct = default
        ) => throw new NotSupportedException();

        public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
            ushort version,
            Enum action,
            object request,
            CancellationToken ct = default
        )
            where TData : notnull => throw new NotSupportedException();

        public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
            ushort version,
            Enum action,
            object request,
            TData defaultValue,
            CancellationToken ct = default
        )
            where TData : notnull => throw new NotSupportedException();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
