using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public static class ByteArraySerializer
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions().ConfigureDefault();

        public static readonly ISerializer<byte[]> Default = Serializer.Create(GetSerialize(DefaultOptions), GetDeserialize(DefaultOptions));

        public static ISerializer<byte[]> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return Serializer.Create(GetSerialize(options), GetDeserialize(options));
        }

        private static Func<object, byte[]> GetSerialize(JsonSerializerOptions options) =>
            value => JsonSerializer.SerializeToUtf8Bytes(value, options);

        private static Func<Type, byte[], object> GetDeserialize(JsonSerializerOptions options) =>
            (type, value) => JsonSerializer.Deserialize(value, type, options);
    }
}