using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Annium.Core.Mapper;
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

        public static void IsEqual<T, TD>(this T value, TD data, string message = "")
        {
            if (!value.IsShallowEqual(data, Mapper.GetFor(Assembly.GetCallingAssembly())))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is not equal to {JsonSerializer.Serialize(data)}"
                        : message
                );
        }

        public static void IsNotEqual<T, TD>(this T value, TD data, string message = "")
        {
            if (value.IsShallowEqual(data, Mapper.GetFor(Assembly.GetCallingAssembly())))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is equal to {JsonSerializer.Serialize(data)}"
                        : message
                );
        }

        public static T As<T>(this object? value, string message = "")
        {
            (value is T).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(T)}" : message);

            return (T) value!;
        }

        public static T AsExact<T>(this object? value, string message = "")
        {
            (value?.GetType() == typeof(T)).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(T)}" : message);

            return (T) value!;
        }

        public static T IsDefault<T>(this T value, string message = "")
        {
            if (!value.IsShallowEqual(default(T)!, Mapper.GetFor(Assembly.GetCallingAssembly())))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is not default"
                        : message
                );

            return value;
        }

        public static T IsNotDefault<T>(this T value, string message = "")
        {
            if (value.IsShallowEqual(default(T)!, Mapper.GetFor(Assembly.GetCallingAssembly())))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is default"
                        : message
                );

            return value;
        }

        public static void IsEqual<T, TD>(this T value, TD data, IMapper mapper, string message)
        {
            if (!value.IsShallowEqual(data, mapper))
                throw new AssertionFailedException(message);
        }

        public static void IsNotEqual<T, TD>(this T value, TD data, IMapper mapper, string message)
        {
            if (value.IsShallowEqual(data, mapper))
                throw new AssertionFailedException(message);
        }
    }
}