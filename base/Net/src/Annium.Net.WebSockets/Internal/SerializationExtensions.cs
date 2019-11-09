using System;
using System.Text.Json;

namespace Annium.Net.WebSockets.Internal
{
    internal static class SerializationExtensions
    {
        public static string Serialize<T>(this T data, MessageFormat format) => format switch
        {
            MessageFormat.Json => JsonSerializer.Serialize(data, Options.Json),
            _ => throw new InvalidOperationException($"Unsupported text format {format}"),
        };

        public static T Deserialize<T>(this ReadOnlyMemory<byte> rawText, MessageFormat format) => format switch
        {
            MessageFormat.Json => JsonSerializer.Deserialize<T>(rawText.Span, Options.Json),
            _ => throw new InvalidOperationException($"Unsupported text format {format}"),
        };
    }
}