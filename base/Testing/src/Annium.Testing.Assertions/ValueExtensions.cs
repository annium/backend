using System.Collections.Generic;

namespace Annium.Testing
{
    public static class ValueExtensions
    {
        public static void IsEqual<T, D>(this T value, D data, string message = "")
        {
            if (!AreEqual(value!, data!))
                throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} != {data}" : message);

            static bool AreEqual(object value, object other)
            {
                return false;
            }
        }

        // public static void IsEqual<T>(this T value, T data, string message = "")
        // {
        //     var comparer = EqualityComparer<T>.Default;

        //     if (!comparer.Equals(value, data))
        //         throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} != {data}" : message);
        // }

        public static void IsNotEqual<T>(this T value, T data, string message = "")
        {
            var comparer = EqualityComparer<T>.Default;

            if (comparer.Equals(value, data))
                throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} == {data}" : message);
        }

        public static object Is<TValue>(this object value, string message = "")
        {
            (value is TValue).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(TValue)}" : message);

            return value!;
        }

        public static object IsNot<TValue>(this object value, string message = "")
        {
            (value is TValue).IsFalse(string.IsNullOrEmpty(message) ? $"{value} is {typeof(TValue)}" : message);

            return value;
        }

        public static TValue As<TValue>(this object value, string message = "") where TValue : class
        {
            (value is TValue).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(TValue)}" : message);

            return (TValue)value!;
        }

        public static T IsDefault<T>(this T value, string message = "")
        {
            value.IsEqual(default(T)!, message);

            return value;
        }

        public static T IsNotDefault<T>(this T value, string message = "")
        {
            value.IsNotEqual(default!, message);

            return value;
        }
    }
}