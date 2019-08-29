using System.Collections.Generic;
using Newtonsoft.Json;

namespace Annium.Data.Operations.Serialization
{
    public static class Extensions
    {
        public static JsonSerializerSettings ConfigureForOperations(this JsonSerializerSettings settings)
        {
            if (settings == null)
                throw new System.ArgumentNullException(nameof(settings));

            AddDefaultConverters(settings.Converters);

            return settings;
        }

        private static void AddDefaultConverters(IList<JsonConverter> converters)
        {
            converters.Add(new ResultConverter());
            converters.Add(new StatusResultConverter());
            converters.Add(new StatusDataResultConverter());
            converters.Add(new BooleanResultConverter());
            converters.Add(new BooleanDataResultConverter());
        }
    }
}