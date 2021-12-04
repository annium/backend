using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Net.Http.Internal;

internal class HttpContentSerializer : IHttpContentSerializer
{
    private readonly IIndex<string, ISerializer<string>> _serializers;

    public HttpContentSerializer(
        IIndex<string, ISerializer<string>> serializers
    )
    {
        _serializers = serializers;
    }

    public bool CanSerialize(string mediaType)
    {
        return _serializers.TryGetValue(mediaType, out _);
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