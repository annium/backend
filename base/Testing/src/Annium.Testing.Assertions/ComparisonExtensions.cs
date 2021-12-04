using System;
using System.Text.Json;

namespace Annium.Testing;

public static class ComparisonExtensions
{
    public static void IsLess<T>(this T value, T data, string message = "")
        where T : IComparable<T>
    {
        if (value.CompareTo(data) >= 0)
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} is not less than {JsonSerializer.Serialize(data)}"
                    : message
            );
    }

    public static void IsLessOrEqual<T>(this T value, T data, string message = "")
        where T : IComparable<T>
    {
        if (value.CompareTo(data) > 0)
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} is not less or equal to {JsonSerializer.Serialize(data)}"
                    : message
            );
    }

    public static void IsGreater<T>(this T value, T data, string message = "")
        where T : IComparable<T>
    {
        if (value.CompareTo(data) <= 0)
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} is not greater than {JsonSerializer.Serialize(data)}"
                    : message
            );
    }

    public static void IsGreaterOrEqual<T>(this T value, T data, string message = "")
        where T : IComparable<T>
    {
        if (value.CompareTo(data) < 0)
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} is not greater or equal to {JsonSerializer.Serialize(data)}"
                    : message
            );
    }
}