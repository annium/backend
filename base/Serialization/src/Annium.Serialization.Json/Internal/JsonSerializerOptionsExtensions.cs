using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Annium.Core.Reflection;

namespace Annium.Serialization.Json.Internal
{
    internal static class JsonSerializerOptionsExtensions
    {
        private static readonly IReadOnlyCollection<PropertyInfo> CloneableProperties = typeof(JsonSerializerOptions)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.PropertyType.GetTargetImplementation(typeof(IEnumerable<>)) is null && x.CanRead && x.CanWrite)
            .ToArray();

        public static JsonSerializerOptions Clone(this JsonSerializerOptions opts)
        {
            var clone = new JsonSerializerOptions();

            foreach (var converter in opts.Converters)
                clone.Converters.Add(converter);

            foreach (var p in CloneableProperties)
                p.SetValue(clone, p.GetValue(opts));

            return clone;
        }
    }
}