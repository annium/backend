using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Net.Http.Internal;

internal class Serializer
{
    private readonly IIndex<string, ISerializer<Stream>> _serializers;

    public Serializer(
        IIndex<string, ISerializer<Stream>> serializers
    )
    {
        _serializers = serializers;
    }

    public Stream Serialize<T>(string mediaType, T value)
    {
        var serializer = ResolveSerializer(mediaType);

        return serializer.Serialize(value);
    }

    public T Deserialize<T>(string mediaType, Stream value)
    {
        var serializer = ResolveSerializer(mediaType);

        return serializer.Deserialize<T>(value);
    }

    private ISerializer<Stream> ResolveSerializer(string mediaType)
    {
        if (_serializers.TryGetValue(mediaType, out var serializer))
            return serializer;

        throw new NotSupportedException($"Media type '{mediaType}' is not supported");
    }
}