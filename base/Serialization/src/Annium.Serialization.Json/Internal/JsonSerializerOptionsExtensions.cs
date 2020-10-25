using System.Text.Json;

namespace Annium.Serialization.Json.Internal
{
    internal static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions Clone(this JsonSerializerOptions opts)
        {
            var clone = new JsonSerializerOptions();

            foreach (var converter in opts.Converters)
                clone.Converters.Add(converter);

            clone.Encoder = opts.Encoder;
            clone.MaxDepth = opts.MaxDepth;
            clone.WriteIndented = opts.WriteIndented;
            clone.AllowTrailingCommas = opts.AllowTrailingCommas;
            clone.DefaultBufferSize = opts.DefaultBufferSize;
            clone.DictionaryKeyPolicy = opts.DictionaryKeyPolicy;
            clone.IgnoreNullValues = opts.IgnoreNullValues;
            clone.PropertyNamingPolicy = opts.PropertyNamingPolicy;
            clone.ReadCommentHandling = opts.ReadCommentHandling;
            clone.IgnoreReadOnlyProperties = opts.IgnoreReadOnlyProperties;
            clone.PropertyNameCaseInsensitive = opts.PropertyNameCaseInsensitive;

            return clone;
        }
    }
}