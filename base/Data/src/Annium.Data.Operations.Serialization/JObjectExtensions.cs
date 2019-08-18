using System;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    internal static class JObjectExtensions
    {
        public static JToken Get(this JObject obj, string key)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Can't get value of empty key", nameof(key));

            return obj.GetValue(key, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}