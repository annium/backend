using System;
using System.Collections.Generic;
using Annium.MessageBus.Abstractions;
using NATS.Client.Core;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// Maps the canonical transport envelope headers to and from NATS message headers. The canonical message id
/// (<see cref="EnvelopeHeaders.Id"/>) is additionally emitted as the NATS-native <c>Nats-Msg-Id</c> header so a
/// JetStream stream deduplicates re-published messages within its duplicate window.
/// </summary>
internal static class NatsHeaderMapper
{
    /// <summary>
    /// The NATS-native message-id header used by JetStream for idempotent publish (deduplication).
    /// </summary>
    private const string NatsMsgIdHeader = "Nats-Msg-Id";

    /// <summary>
    /// Builds NATS headers from the canonical envelope headers, mirroring the message id into <c>Nats-Msg-Id</c>.
    /// </summary>
    /// <param name="headers">The canonical envelope + user headers.</param>
    /// <returns>The NATS headers to attach to the published message.</returns>
    public static NatsHeaders ToNatsHeaders(IReadOnlyDictionary<string, string> headers)
    {
        var natsHeaders = new NatsHeaders();
        foreach (var (key, value) in headers)
            natsHeaders[key] = value;

        if (headers.TryGetValue(EnvelopeHeaders.Id, out var id) && !string.IsNullOrEmpty(id))
            natsHeaders[NatsMsgIdHeader] = id;

        return natsHeaders;
    }

    /// <summary>
    /// Decodes NATS headers into a canonical header dictionary, dropping the NATS-native <c>Nats-Msg-Id</c> mirror
    /// (the canonical id is already carried under <see cref="EnvelopeHeaders.Id"/>).
    /// </summary>
    /// <param name="headers">The received NATS headers (may be null when the message carried none).</param>
    /// <returns>The canonical headers.</returns>
    public static Dictionary<string, string> FromNatsHeaders(NatsHeaders? headers)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (headers is null)
            return result;

        foreach (var key in headers.Keys)
        {
            if (string.Equals(key, NatsMsgIdHeader, StringComparison.Ordinal))
                continue;
            result[key] = headers[key].ToString();
        }

        return result;
    }
}
