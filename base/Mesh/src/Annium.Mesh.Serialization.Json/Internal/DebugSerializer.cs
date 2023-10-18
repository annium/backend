using System;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Serialization.Json.Internal;

internal class DebugSerializer : ISerializer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly ISerializer<string> _serializer;

    public DebugSerializer(
        IIndex<SerializerKey, ISerializer<string>> serializers,
        ILogger logger
    )
    {
        Logger = logger;
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = serializers[key];
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        var str = _serializer.Serialize(value);
        this.Trace<string>("string: {value}", str);

        var raw = Encoding.UTF8.GetBytes(str);
        this.Trace<int, string>("raw({length}): {value}", raw.Length, string.Join(',', raw));

        return raw;
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        this.Trace<int, string>("raw({length}): {value}", data.Length, string.Join(',', data.ToArray()));

        var str = Encoding.UTF8.GetString(data.Span);
        this.Trace<string>("string: {value}", str);

        var value = _serializer.Deserialize<T>(str);

        return value;
    }
}