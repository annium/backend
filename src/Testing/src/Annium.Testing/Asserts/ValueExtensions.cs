using System.Collections.Generic;

namespace Annium.Testing
{
    public static class ValueExtensions
    {
        public static void IsEqual<T>(this T value, T data, string message = null)
        {
            var comparer = EqualityComparer<T>.Default;

            if (!comparer.Equals(value, data))
                throw new AssertionFailedException(message ?? $"{value} != {data}");
        }

        public static void IsNotEqual<T>(this T value, T data, string message = null)
        {
            var comparer = EqualityComparer<T>.Default;

            if (comparer.Equals(value, data))
                throw new AssertionFailedException(message ?? $"{value} == {data}");
        }

        public static object Is<TValue>(this object value, string message = null)
        {
            (value is TValue).IsTrue(message);

            return value;
        }

        public static object IsNot<TValue>(this object value, string message = null)
        {
            (value is TValue).IsFalse(message);

            return value;
        }

        public static TValue As<TValue>(this object value, string message = null) where TValue : class
        {
            (value is TValue).IsTrue(message);

            return value as TValue;
        }

        public static T IsDefault<T>(this T value, string message = null)
        {
            value.IsEqual(default(T), message);

            return value;
        }

        public static T IsNotDefault<T>(this T value, string message = null)
        {
            value.IsNotEqual(default(T), message);

            return value;
        }
    }
}