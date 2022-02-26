using System.Text.Json;

namespace Annium.Testing.Assertions.Internal;

internal static class HelperExtensions
{
    public static string Str<T>(this T value) => JsonSerializer.Serialize(value);

    public static string Wrap<T>(this T value, string ex)
    {
        var v = JsonSerializer.Serialize(value);

        return v == ex ? v : $"{ex} ({v})";
    }
}