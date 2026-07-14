using System;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// The default <see cref="INatsConfigurationBuilder"/> implementation. Parses and validates the server URL eagerly
/// and builds an immutable <see cref="NatsConfiguration"/>.
/// </summary>
internal sealed class NatsConfigurationBuilder : INatsConfigurationBuilder
{
    /// <summary>
    /// The parsed server URL, if configured.
    /// </summary>
    private Uri? _url;

    /// <summary>
    /// Sets the NATS server URL, parsing and validating it eagerly (must be an absolute URI with scheme
    /// <c>nats</c>, <c>tls</c>, <c>ws</c> or <c>wss</c>).
    /// </summary>
    /// <param name="url">The server URL.</param>
    /// <returns>The builder for method chaining.</returns>
    public INatsConfigurationBuilder Url(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("NATS server URL must be a non-empty string.", nameof(url));
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
            throw new ArgumentException($"NATS server URL '{url}' is not a valid absolute URI.", nameof(url));
        if (parsed.Scheme is not ("nats" or "tls" or "ws" or "wss"))
            throw new ArgumentException(
                $"NATS server URL scheme must be 'nats', 'tls', 'ws' or 'wss', got '{parsed.Scheme}'.",
                nameof(url)
            );

        _url = parsed;
        return this;
    }

    /// <summary>
    /// Builds the immutable configuration.
    /// </summary>
    /// <returns>The resolved configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server URL was not configured.</exception>
    public NatsConfiguration Build()
    {
        if (_url is null)
            throw new InvalidOperationException("NATS server URL must be configured.");

        return new NatsConfiguration { Url = _url };
    }
}
