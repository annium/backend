using System;
using System.Text.Json.Serialization;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal static class MessageBusEnvelope
    {
        public static MessageBusEnvelope<T> Data<T>(Guid id, T data) =>
            new(id, data, string.Empty);

        public static MessageBusEnvelope<object> Error(Guid id, string error) =>
            new(id, default!, error);
    }

    internal class MessageBusEnvelope<T>
    {
        [JsonPropertyName("i")]
        public Guid Id { get; }

        [JsonPropertyName("d")]
        public T Data { get; }

        [JsonPropertyName("e")]
        public string Error { get; }

        public bool IsSuccess => string.IsNullOrWhiteSpace(Error);

        public MessageBusEnvelope(
            Guid id,
            T data,
            string error
        )
        {
            Id = id;
            Data = data;
            Error = error;
        }
    }
}