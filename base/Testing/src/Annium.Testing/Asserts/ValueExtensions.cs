using System.Collections.Generic;

namespace Annium.Testing
{
    public static class ValueExtensions
    {
        public static void IsEqual<T>(this T value, T data, string message = "")
        {
            var comparer = EqualityComparer<T>.Default;

            if (!comparer.Equals(value, data))
                throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} != {data}" : message);
        }

        public static void IsNotEqual<T>(this T value, T data, string message = "")
        {
            var comparer = EqualityComparer<T>.Default;

            if (comparer.Equals(value, data))
                throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} == {data}" : message);
        }

        public static object Is<TValue>(this object value, string message = "")
        {
            (value is TValue).IsTrue(message);

            return value;
        }

        public static object IsNot<TValue>(this object value, string message = "")
        {
            (value is TValue).IsFalse(message);

            return value;
        }

        public static TValue As<TValue>(this object value, string message = "") where TValue : class
        {
            (value is TValue).IsTrue(message);

            return (TValue) value;
        }

        public static T IsDefault<T>(this T value, string message = "")
        {
            value.IsEqual(default !, message);

            return value;
        }

        public static T IsNotDefault<T>(this T value, string message = "")
        {
            value.IsNotEqual(default !, message);

            return value;
        }
    }
}