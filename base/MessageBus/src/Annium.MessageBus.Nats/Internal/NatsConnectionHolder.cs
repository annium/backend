using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// Owns the single shared <see cref="NatsConnection"/> and its JetStream context for one broker configuration.
/// Registered as a DI singleton by <c>AddNatsMessageBus</c> (disposed by the container); both the producer and every
/// consumer created by <see cref="NatsTransport"/> use it. The connection is created and connected lazily under a gate
/// on first use, and disposal is idempotent (the holder is registered under a single service type, but disposal races
/// are still guarded).
/// </summary>
internal sealed class NatsConnectionHolder : IAsyncDisposable, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// The adapter configuration.
    /// </summary>
    private readonly NatsConfiguration _config;

    /// <summary>
    /// Serializes lazy connection creation.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// The shared connection, created lazily.
    /// </summary>
    private NatsConnection? _connection;

    /// <summary>
    /// The JetStream context over the shared connection, created lazily.
    /// </summary>
    private NatsJSContext? _jetStream;

    /// <summary>
    /// Guards against repeated disposal.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NatsConnectionHolder"/> class.
    /// </summary>
    /// <param name="config">The adapter configuration.</param>
    /// <param name="logger">The logger.</param>
    public NatsConnectionHolder(NatsConfiguration config, ILogger logger)
    {
        _config = config;
        Logger = logger;
    }

    /// <summary>
    /// Returns the shared connection, creating and connecting it on first call.
    /// </summary>
    /// <param name="ct">A token to cancel connection.</param>
    /// <returns>The connected NATS connection.</returns>
    public async ValueTask<NatsConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is { } existing)
            return existing;

        await _gate.WaitAsync(ct);
        try
        {
            if (_connection is null)
            {
                var opts = new NatsOpts
                {
                    Url = _config.Url.ToString(),
                    Name = "annium.messagebus",
                    // Drain in-flight subscription messages on connection dispose (graceful shutdown; minimizes
                    // redelivery). Per-subscription drain is additionally handled by the shared consumption pipeline.
                    DrainSubscriptionsOnDispose = true,
                };
                var connection = new NatsConnection(opts);
                await connection.ConnectAsync();
                _connection = connection;
                _jetStream = new NatsJSContext(connection);
            }
        }
        finally
        {
            _gate.Release();
        }

        return _connection;
    }

    /// <summary>
    /// Returns the JetStream context over the shared connection, creating the connection on first call.
    /// </summary>
    /// <param name="ct">A token to cancel connection.</param>
    /// <returns>The JetStream context.</returns>
    public async ValueTask<NatsJSContext> GetJetStreamAsync(CancellationToken ct)
    {
        await GetConnectionAsync(ct);
        return _jetStream!;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (_connection is { } connection)
        {
            try
            {
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                this.Error<string>("nats connection dispose failed: {error}", e.Message);
            }
        }

        _gate.Dispose();
    }
}
