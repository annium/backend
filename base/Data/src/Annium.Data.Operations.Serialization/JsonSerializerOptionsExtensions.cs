using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Data.Operations.Serialization;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureForOperations(
            this JsonSerializerOptions options
        )
        {
            AddDefaultConverters(options.Converters);

            return options;
        }

        private static void AddDefaultConverters(IList<JsonConverter> converters)
        {
            converters.Add(new ResultConverterFactory());
            converters.Add(new ResultDataConverterFactory());
            converters.Add(new StatusResultConverterFactory());
            converters.Add(new StatusDataResultConverterFactory());
            converters.Add(new BooleanResultConverterFactory());
            converters.Add(new BooleanDataResultConverterFactory());
        }
    }
}