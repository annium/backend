using System;
using System.Linq;
using Annium.Core.Application.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Serialization.Json
{
    public class AbstractJsonConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => TypeManager.Instance.CanResolve(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                return existingValue;

            var obj = JObject.Load(reader);
            objectType = getRealType(obj, objectType);

            existingValue = Activator.CreateInstance(objectType);
            serializer.Populate(obj.CreateReader(), existingValue);

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();

        private Type getRealType(JObject obj, Type objectType)
        {
            var properties = obj.Properties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray();

            return TypeManager.Instance.ResolveBySignature(properties, objectType, true);
        }
    }
}