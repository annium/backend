using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// Parses and formats Kafka bootstrap-servers: a comma-separated <c>host:port</c> list ⇄ a list of
/// <see cref="KafkaEndpoint"/>. <see cref="Format"/> is the inverse of <see cref="Parse"/> up to normalization.
/// </summary>
internal static class BootstrapServersParser
{
    /// <summary>
    /// Parses and validates a comma-separated <c>host:port</c> list into endpoints. A scheme prefix
    /// (<c>scheme://</c>) and a trailing path are stripped, surrounding whitespace is trimmed, empty entries are
    /// dropped, and every entry must have a non-empty host and an integer port in 1-65535.
    /// </summary>
    /// <param name="servers">The raw bootstrap-servers string.</param>
    /// <returns>The parsed endpoints.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is empty or any entry is not a valid <c>host:port</c>.</exception>
    public static IReadOnlyList<KafkaEndpoint> Parse(string servers)
    {
        if (string.IsNullOrWhiteSpace(servers))
            throw new ArgumentException("Kafka bootstrap servers must be a non-empty host:port list.", nameof(servers));

        var entries = servers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (entries.Length == 0)
            throw new ArgumentException(
                "Kafka bootstrap servers must contain at least one host:port entry.",
                nameof(servers)
            );

        return entries.Select(entry => ParseEntry(entry, nameof(servers))).ToArray();
    }

    /// <summary>
    /// Formats endpoints back into a canonical comma-separated <c>host:port</c> list.
    /// </summary>
    /// <param name="endpoints">The endpoints to format.</param>
    /// <returns>The <c>host:port,host:port</c> string.</returns>
    public static string Format(IEnumerable<KafkaEndpoint> endpoints) => string.Join(",", endpoints);

    /// <summary>
    /// Parses and validates a single <c>host:port</c> entry.
    /// </summary>
    /// <param name="entry">The raw entry.</param>
    /// <param name="paramName">The parameter name for exceptions.</param>
    /// <returns>The parsed endpoint.</returns>
    /// <exception cref="ArgumentException">Thrown when the entry is not a valid <c>host:port</c>.</exception>
    private static KafkaEndpoint ParseEntry(string entry, string paramName)
    {
        // strip an optional scheme (e.g. PLAINTEXT://host:port) — librdkafka expects a bare host:port
        var address = entry;
        var schemeIndex = address.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex >= 0)
            address = address[(schemeIndex + 3)..];

        // strip an optional trailing path (URI-style addresses may end with '/')
        var pathIndex = address.IndexOf('/');
        if (pathIndex >= 0)
            address = address[..pathIndex];

        // split on the last colon so bracketed IPv6 hosts (e.g. [::1]:9092) keep their inner colons
        var colonIndex = address.LastIndexOf(':');
        if (colonIndex <= 0 || colonIndex == address.Length - 1)
            throw new ArgumentException($"Invalid Kafka bootstrap server '{entry}': expected 'host:port'.", paramName);

        var host = address[..colonIndex];
        var portText = address[(colonIndex + 1)..];
        if (host.Contains(' '))
            throw new ArgumentException(
                $"Invalid Kafka bootstrap server '{entry}': host must not contain whitespace.",
                paramName
            );

        if (
            !int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out var port)
            || port is < 1 or > 65535
        )
            throw new ArgumentException(
                $"Invalid Kafka bootstrap server '{entry}': port must be an integer in 1-65535.",
                paramName
            );

        return new KafkaEndpoint(host, port);
    }
}
