using System.Collections.Generic;
using System.Text.Json;

namespace Annium.Testing;

public static class EqualityExtensions
{
    public static void Is<T>(this T value, T data, string message = "")
    {
        if (!EqualityComparer<T>.Default.Equals(value, data))
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} != {JsonSerializer.Serialize(data)}"
                    : message
            );
    }

    public static void IsNot<T>(this T value, T data, string message = "")
    {
        if (EqualityComparer<T>.Default.Equals(value, data))
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} == {JsonSerializer.Serialize(data)}"
                    : message
            );
    }
}