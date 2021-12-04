using System.Text.Json;

namespace Annium.Serialization.Json.Internal;

internal class OptionsContainer
{
    public JsonSerializerOptions Value { get; }

    public OptionsContainer(JsonSerializerOptions value)
    {
        Value = value;
    }
}