using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Annium.Data.Models.Extensions;

namespace Annium.Testing
{
    public static class ValueExtensions
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

        public static void IsEqual<T, D>(this T value, D data, string message = "")
        {
            if (!AreEqual(value, data))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is not equal to {JsonSerializer.Serialize(data)}"
                        : message
                );
        }

        public static void IsNotEqual<T, D>(this T value, D data, string message = "")
        {
            if (AreEqual(value, data))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is equal to {JsonSerializer.Serialize(data)}"
                        : message
                );
        }

        public static T As<T>(this object value, string message = "") where T : class
        {
            (value is T).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(T)}" : message);

            return (T) value!;
        }

        public static T IsDefault<T>(this T value, string message = "")
        {
            value.IsEqual(default(T)!, message);

            return value;
        }

        public static T IsNotDefault<T>(this T value, string message = "")
        {
            value.IsNotEqual(default(T)!, message);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreEqual<T, D>(T value, D data)
        {
            return value.IsShallowEqual(data);
        }
    }
}