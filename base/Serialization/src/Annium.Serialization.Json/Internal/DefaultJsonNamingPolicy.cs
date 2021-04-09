using System.Text.Json;

namespace Annium.Serialization.Json.Internal
{
    internal class DefaultJsonNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name;
    }
}