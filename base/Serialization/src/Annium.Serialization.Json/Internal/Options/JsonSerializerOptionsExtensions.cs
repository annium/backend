using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;

namespace Annium.Serialization.Json.Internal.Options;

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

    public static JsonSerializerOptions RemoveConverter<T>(this JsonSerializerOptions opts)
        where T : JsonConverter
    {
        var factory = opts.Converters.Single(x => x.GetType() == typeof(T));
        opts.Converters.Remove(factory);

        return opts;
    }
}