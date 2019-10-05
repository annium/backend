using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Annium.Data.Operations.Serialization
{
    internal static class HelperExtensions
    {
        public static bool HasProperty(this ref Utf8JsonReader reader, string property)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                return false;

            var name = reader.GetString();

            return name.Equals(property, StringComparison.InvariantCultureIgnoreCase);
        }

        public static object Get(this object obj, string property)
        {
            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("Can't get value by empty property name", nameof(property));

            return obj.GetType().GetProperty(property) !.GetGetMethod() !.Invoke(obj, Array.Empty<object>()) !;
        }

        public static void Errors(this IResultBase obj, IEnumerable<string> errors)
        {
            obj.Errors<IEnumerable<string>>(errors);
        }

        public static void Errors(this IResultBase obj, IReadOnlyDictionary<string, IEnumerable<string>> errors)
        {
            obj.Errors<IReadOnlyDictionary<string, IEnumerable<string>>>(errors);
        }

        private static void Errors<T>(this IResultBase obj, T errors)
        {
            obj.GetType()
                .GetMethod(nameof(IResultBase<object>.Errors), new [] { errors!.GetType() }) !
                .Invoke(obj, new object[] { errors });
        }
    }
}