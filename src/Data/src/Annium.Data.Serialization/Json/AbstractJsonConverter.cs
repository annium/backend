using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Serialization.Json
{
    public class AbstractJsonConverter : JsonConverter
    {
        private readonly IDictionary<Type, Type[]> types;

        private readonly IDictionary<Type, string[]> signatures;

        public AbstractJsonConverter(params string[] matches)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (matches.Length > 0)
            {
                matches = matches.Select(m => m.ToLowerInvariant()).ToArray();
                assemblies = assemblies.Where(a =>
                {
                    var name = a.GetName().Name.ToLowerInvariant();
                    return matches.Any(m => name.Contains(m));
                }).ToArray();
            }

            var types = assemblies
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .ToArray();

            this.types = types
                .ToDictionary(
                    t => t,
                    t => types.Where(s => s != t && t.IsAssignableFrom(s) && s.IsClass && !s.IsAbstract).ToArray()
                )
                .Where(p => p.Value.Length > 0)
                .ToDictionary(p => p.Key, p => p.Value);

            this.signatures = this.types.Values
                .SelectMany(v => v)
                .Distinct()
                .ToDictionary(
                    t => t,
                    t => t.GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray()
                )
                .Where(p => p.Value.Length > 0)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => types.ContainsKey(objectType);

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

            return types[objectType].FirstOrDefault(type => properties.SequenceEqual(signatures[type])) ?? objectType;
        }
    }
}