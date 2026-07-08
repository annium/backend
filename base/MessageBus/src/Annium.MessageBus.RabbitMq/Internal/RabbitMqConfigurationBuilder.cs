using System;

namespace Annium.MessageBus.RabbitMq.Internal;

/// <summary>
/// The default <see cref="IRabbitMqConfigurationBuilder"/> implementation. Parses and validates the connection URI
/// eagerly and builds an immutable <see cref="RabbitMqConfiguration"/>.
/// </summary>
internal sealed class RabbitMqConfigurationBuilder : IRabbitMqConfigurationBuilder
{
    /// <summary>
    /// The default topic exchange name.
    /// </summary>
    private const string DefaultExchange = "annium.messagebus";

    /// <summary>
    /// The parsed connection URI, if configured.
    /// </summary>
    private Uri? _connectionUri;

    /// <summary>
    /// The exchange name (defaults to <see cref="DefaultExchange"/>).
    /// </summary>
    private string _exchange = DefaultExchange;

    /// <inheritdoc />
    public IRabbitMqConfigurationBuilder ConnectionUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("RabbitMQ connection URI must be a non-empty string.", nameof(uri));
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            throw new ArgumentException($"RabbitMQ connection URI '{uri}' is not a valid absolute URI.", nameof(uri));
        if (parsed.Scheme is not ("amqp" or "amqps"))
            throw new ArgumentException(
                $"RabbitMQ connection URI scheme must be 'amqp' or 'amqps', got '{parsed.Scheme}'.",
                nameof(uri)
            );

        _connectionUri = parsed;
        return this;
    }

    /// <inheritdoc />
    public IRabbitMqConfigurationBuilder Exchange(string exchange)
    {
        if (string.IsNullOrWhiteSpace(exchange))
            throw new ArgumentException("RabbitMQ exchange name must be a non-empty string.", nameof(exchange));

        _exchange = exchange;
        return this;
    }

    /// <summary>
    /// Builds the immutable configuration.
    /// </summary>
    /// <returns>The resolved configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the connection URI was not configured.</exception>
    public RabbitMqConfiguration Build()
    {
        if (_connectionUri is null)
            throw new InvalidOperationException("RabbitMQ connection URI must be configured.");

        return new RabbitMqConfiguration { ConnectionUri = _connectionUri, ExchangeName = _exchange };
    }
}
