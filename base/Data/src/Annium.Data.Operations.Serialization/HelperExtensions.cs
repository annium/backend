using System;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    internal static class HelperExtensions
    {
        public static JToken Get(this JObject obj, string key)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Can't get value of empty key", nameof(key));

            return obj.GetValue(key, StringComparison.InvariantCultureIgnoreCase);
        }

        public static object GetPropertyValue(this object obj, string property)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("Can't get value by empty property name", nameof(property));

            return obj.GetType().GetProperty(property) !.GetGetMethod() !.Invoke(obj, Array.Empty<object>()) !;
        }
    }
}