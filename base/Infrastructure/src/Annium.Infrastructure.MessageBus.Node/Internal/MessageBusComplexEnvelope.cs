using System;
using System.Text.Json.Serialization;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

// TODO: use in case of request/response multipart envelopes
internal static class MessageBusComplexEnvelope
{
    public static MessageBusComplexEnvelope<T> Single<T>(T data) =>
        new(Guid.NewGuid(), EnvelopeType.Data | EnvelopeType.Final, data, string.Empty);

    public static MessageBusComplexEnvelope<T> Single<T>(string error) =>
        new(Guid.NewGuid(), EnvelopeType.Error | EnvelopeType.Final, default!, error);

    public static MessageBusComplexEnvelope<T> Chunk<T>(Guid id, T data) =>
        new(id, EnvelopeType.Data, data, string.Empty);

    public static MessageBusComplexEnvelope<T> Chunk<T>(Guid id, string error) =>
        new(id, EnvelopeType.Error, default!, error);

    public static MessageBusComplexEnvelope<T> End<T>(Guid id, T data) =>
        new(id, EnvelopeType.Data | EnvelopeType.Final, data, string.Empty);

    public static MessageBusComplexEnvelope<T> End<T>(Guid id, string error) =>
        new(id, EnvelopeType.Data | EnvelopeType.Final, default!, error);
}

internal class MessageBusComplexEnvelope<T>
{
    [JsonPropertyName("i")]
    public Guid Id { get; }

    [JsonPropertyName("t")]
    public EnvelopeType Type { get; }

    [JsonPropertyName("d")]
    public T Data { get; }

    [JsonPropertyName("e")]
    public string Error { get; }

    public MessageBusComplexEnvelope(
        Guid id,
        EnvelopeType type,
        T data,
        string error
    )

    {
        Id = id;
        Type = type;
        Data = data;
        Error = error;
    }
}

[Flags]
internal enum EnvelopeType
{
    Final = 1,
    Data = 2,
    Error = 4,
}