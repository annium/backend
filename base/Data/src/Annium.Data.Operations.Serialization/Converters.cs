using Newtonsoft.Json;

namespace Annium.Data.Operations.Serialization
{
    public static class Converters
    {
        public static JsonConverter BooleanResultConverter { get; } = new BooleanResultConverter();
 
        public static JsonConverter StatusResultConverter { get; } = new StatusResultConverter();
    }
}