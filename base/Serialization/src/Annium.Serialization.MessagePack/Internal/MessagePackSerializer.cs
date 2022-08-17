using System;
using System.Text;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

internal class MessagePackSerializer : ISerializer<ReadOnlyMemory<byte>>, ILogSubject<MessagePackSerializer>
{
    public ILogger<MessagePackSerializer> Logger { get; }

    public MessagePackSerializer(
        ILogger<MessagePackSerializer> logger
    )
    {
        Logger = logger;
    }

    private readonly MessagePackSerializerOptions _opts =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public T Deserialize<T>(ReadOnlyMemory<byte> value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Deserialize<T>(value, _opts);
        }
        catch (Exception e)
        {
            this.Log().Error("Failed to deserialize {value} as {type} with {error}", Encoding.UTF8.GetString(value.ToArray()), typeof(T).FriendlyName(), e);
            throw;
        }
    }

    public object? Deserialize(Type type, ReadOnlyMemory<byte> value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Deserialize(type, value, _opts);
        }
        catch (Exception e)
        {
            this.Log().Error("Failed to deserialize {value} as {type} with {error}", Encoding.UTF8.GetString(value.ToArray()), type.FriendlyName(), e);
            throw;
        }
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, _opts);
        }
        catch (Exception e)
        {
            this.Log().Error("Failed to serialize {value} as {type} with {error}", value?.ToString() ?? (object) "null", typeof(T).FriendlyName(), e);
            throw;
        }
    }

    public ReadOnlyMemory<byte> Serialize(object? value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, _opts);
        }
        catch (Exception e)
        {
            this.Log().Error("Failed to serialize {value} as {type} with {error}", value!, value?.GetType().FriendlyName() ?? (object) "null", e);
            throw;
        }
    }
}