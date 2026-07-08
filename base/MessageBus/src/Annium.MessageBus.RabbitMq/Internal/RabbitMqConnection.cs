using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using RabbitMQ.Client;

namespace Annium.MessageBus.RabbitMq.Internal;

/// <summary>
/// Owns the single shared AMQP connection (with automatic connection and topology recovery) used by the producer
/// channel and every consumer channel. Created and disposed by DI. Channels are opened through
/// <see cref="CreateChannelAsync"/>, which also declares the durable topic exchange on each new channel (idempotent),
/// so the topology is re-established after a recovery.
/// </summary>
internal sealed class RabbitMqConnection : IAsyncDisposable, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the durable topic exchange name all subjects route through.
    /// </summary>
    public string ExchangeName => _config.ExchangeName;

    /// <summary>
    /// The adapter configuration.
    /// </summary>
    private readonly RabbitMqConfiguration _config;

    /// <summary>
    /// Serializes lazy connection creation.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// The shared connection (created lazily on first channel request). Automatic recovery keeps this instance valid
    /// across broker outages, so it is created once and never replaced.
    /// </summary>
    private IConnection? _connection;

    /// <summary>
    /// Guards against repeated disposal.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConnection"/> class.
    /// </summary>
    /// <param name="config">The adapter configuration.</param>
    /// <param name="logger">The logger.</param>
    public RabbitMqConnection(RabbitMqConfiguration config, ILogger logger)
    {
        _config = config;
        Logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch (Exception e)
            {
                this.Error<string>("rabbitmq connection close failed: {error}", e.Message);
            }

            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }

    /// <summary>
    /// Opens a fresh channel on the shared connection and declares the durable topic exchange on it.
    /// </summary>
    /// <param name="options">The channel options (publisher confirms, consumer dispatch concurrency).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The opened channel with the exchange declared.</returns>
    public async Task<IChannel> CreateChannelAsync(CreateChannelOptions options, CancellationToken ct)
    {
        var connection = await GetConnectionAsync(ct);
        var channel = await connection.CreateChannelAsync(options, ct);
        await channel.ExchangeDeclareAsync(
            _config.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct
        );
        return channel;
    }

    /// <summary>
    /// Returns the shared connection, creating it on first use.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The shared connection.</returns>
    private async Task<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is not null)
            return _connection;

        await _gate.WaitAsync(ct);
        try
        {
            if (_connection is not null)
                return _connection;

            var factory = new ConnectionFactory
            {
                Uri = _config.ConnectionUri,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(2),
            };
            _connection = await factory.CreateConnectionAsync(ct);

            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }
}
