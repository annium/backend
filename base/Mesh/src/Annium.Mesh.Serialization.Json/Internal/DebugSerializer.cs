using System;
using System.Text;
using System.Text.Json;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Serialization.Json.Internal;

internal class DebugSerializer : ISerializer, ILogSubject
{
    private static readonly JsonSerializerOptions _messageOpts;

    static DebugSerializer()
    {
        _messageOpts = new JsonSerializerOptions();
        _messageOpts.Converters.Add(new MessageConverter());
    }

    public ILogger Logger { get; }
    private readonly ISerializer<string> _serializer;

    public DebugSerializer(IServiceProvider sp, ILogger logger)
    {
        Logger = logger;
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(key);
    }

    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        this.Trace("start: {message}", message);

        var data = JsonSerializer.SerializeToUtf8Bytes(message, _messageOpts);

        this.Trace("done: {length} bytes", data.Length);

        return data;
    }

    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        this.Trace("start: {size} bytes", data.Length);

        var message = JsonSerializer.Deserialize<Message>(data.Span, _messageOpts)!;

        this.Trace("done: {message}", message);

        return message;
    }

    public ReadOnlyMemory<byte> SerializeData(Type type, object? value)
    {
        var str = _serializer.Serialize(type, value);
        this.Trace<string>("string: {value}", str);

        var raw = Encoding.UTF8.GetBytes(str);
        this.Trace<int, string>("raw({length}): {value}", raw.Length, string.Join(',', raw));

        return raw;
    }

    public object? DeserializeData(Type type, ReadOnlyMemory<byte> data)
    {
        this.Trace<int, string>("raw({length}): {value}", data.Length, string.Join(',', data.ToArray()));

        var str = Encoding.UTF8.GetString(data.Span);
        this.Trace<string>("string: {value}", str);

        var value = _serializer.Deserialize(type, str);

        return value;
    }
}
