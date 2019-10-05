using System.Text.Json;
using Annium.Data.Serialization.Json;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureAbstractConverter(
            this JsonSerializerOptions options
        )
        {
            options.Converters.Add(new AbstractJsonConverterFactory());

            return options;
        }
    }
}