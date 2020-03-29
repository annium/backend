using System.Runtime.CompilerServices;
using System.Text.Json;
using Annium.Data.Models.Extensions;

namespace Annium.Testing
{
    public static class ValueExtensions
    {
        public static void IsEqual<T, D>(this T value, D data, string message = "")
        {
            if (!AreEqual(value, data))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} != {JsonSerializer.Serialize(data)}"
                        : message
                );
        }

        public static void IsNotEqual<T, D>(this T value, D data, string message = "")
        {
            if (AreEqual(value, data))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} == {JsonSerializer.Serialize(data)}"
                        : message
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreEqual<T, D>(T value, D data)
        {
            return value.IsShallowEqual(data);
        }

        public static object Is<TValue>(this object value, string message = "")
        {
            (value is TValue).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(TValue)}" : message);

            return value!;
        }

        public static TValue As<TValue>(this object value, string message = "") where TValue : class
        {
            (value is TValue).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(TValue)}" : message);

            return (TValue) value!;
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
    }
}