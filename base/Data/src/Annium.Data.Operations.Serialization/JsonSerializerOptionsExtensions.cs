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
            converters.Add(new ResultConverter());
            converters.Add(new ResultDataConverter());
            converters.Add(new StatusResultConverter());
            converters.Add(new StatusDataResultConverter());
            converters.Add(new BooleanResultConverter());
            converters.Add(new BooleanDataResultConverter());
        }
    }
}