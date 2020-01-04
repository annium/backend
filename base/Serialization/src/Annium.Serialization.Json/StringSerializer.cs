using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public static class StringSerializer
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions().ConfigureDefault();

        public static readonly ISerializer<string> Default = Serializer.Create(GetSerialize(DefaultOptions), GetDeserialize(DefaultOptions));

        public static ISerializer<string> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return Serializer.Create(GetSerialize(options), GetDeserialize(options));
        }

        private static Func<object, string> GetSerialize(JsonSerializerOptions options) =>
            value => JsonSerializer.Serialize(value, options);

        private static Func<Type, string, object> GetDeserialize(JsonSerializerOptions options) =>
            (type, value) => JsonSerializer.Deserialize(value, type, options);
    }
}