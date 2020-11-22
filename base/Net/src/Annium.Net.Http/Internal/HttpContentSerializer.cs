using System;
using System.Collections.Generic;
using System.Net.Mime;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using NodaTime.Xml;

namespace Annium.Net.Http.Internal
{
    internal class HttpContentSerializer : IHttpContentSerializer
    {
        private readonly IReadOnlyDictionary<string, ISerializer<string>> _serializers;

        public HttpContentSerializer(
            ITypeManager typeManager,
            ISerializer<string> serializer
        )
        {
            var serializers = new Dictionary<string, ISerializer<string>>();
            serializers[MediaTypeNames.Application.Json] = StringSerializer.Configure(opts => opts
                .ConfigureDefault(typeManager)
                .ConfigureForOperations()
                .ConfigureForNodaTime(XmlSerializationSettings.DateTimeZoneProvider));
            _serializers = serializers;
        }

        public bool CanSerialize(string mediaType)
        {
            return _serializers.ContainsKey(mediaType);
        }

        public string Serialize<T>(string mediaType, T value)
        {
            var serializer = ResolveSerializer(mediaType);

            return serializer.Serialize(value);
        }

        public T Deserialize<T>(string mediaType, string value)
        {
            var serializer = ResolveSerializer(mediaType);

            return serializer.Deserialize<T>(value);
        }

        private ISerializer<string> ResolveSerializer(string mediaType)
        {
            if (_serializers.TryGetValue(mediaType, out var serializer))
                return serializer;

            throw new NotSupportedException($"Media type '{mediaType}' is not supported");
        }
    }
}