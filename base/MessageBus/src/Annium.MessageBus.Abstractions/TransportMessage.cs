using System.Collections.Generic;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// A transport-agnostic outbound message produced by the shared pipeline. The <see cref="Body"/> is the already
/// serialized payload (a <see cref="string"/>); each adapter is responsible for the final UTF-8 → bytes step and
/// for mapping the canonical <see cref="Headers"/> (see <see cref="EnvelopeHeaders"/>) onto its broker-native
/// representation.
/// </summary>
/// <param name="Subject">The canonical destination subject.</param>
/// <param name="Body">The serialized payload.</param>
/// <param name="Headers">The canonical envelope + user headers.</param>
/// <param name="Key">The optional partition/ordering key (native only on Kafka; best-effort elsewhere).</param>
public sealed record TransportMessage(
    string Subject,
    string Body,
    IReadOnlyDictionary<string, string> Headers,
    string? Key
);
