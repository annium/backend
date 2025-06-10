using System;
using System.Text;
using System.Text.Json;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Serialization.Json.Internal;

/// <summary>
/// Provides JSON serialization implementation with detailed debug logging for troubleshooting mesh communication.
/// </summary>
internal class DebugSerializer : ISerializer, ILogSubject
{
    /// <summary>
    /// JSON serializer options configured specifically for mesh message serialization.
    /// </summary>
    private static readonly JsonSerializerOptions _messageOpts;

    /// <summary>
    /// Static constructor that initializes the JSON serializer options with the custom message converter.
    /// </summary>
    static DebugSerializer()
    {
        _messageOpts = new JsonSerializerOptions();
        _messageOpts.Converters.Add(new MessageConverter());
    }

    /// <summary>
    /// Gets the logger instance for debug output.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The underlying serializer used for data payload serialization.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// Initializes a new instance of the DebugSerializer class.
    /// </summary>
    /// <param name="sp">The service provider used to resolve the underlying serializer.</param>
    /// <param name="logger">The logger instance for debug output.</param>
    public DebugSerializer(IServiceProvider sp, ILogger logger)
    {
        Logger = logger;
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(key);
    }

    /// <summary>
    /// Serializes a mesh message to JSON format as binary data with debug logging.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The serialized message as UTF-8 encoded JSON bytes.</returns>
    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        this.Trace("start: {message}", message);

        var data = JsonSerializer.SerializeToUtf8Bytes(message, _messageOpts);

        this.Trace("done: {length} bytes", data.Length);

        return data;
    }

    /// <summary>
    /// Deserializes JSON binary data back to a mesh message with debug logging.
    /// </summary>
    /// <param name="data">The JSON binary data to deserialize.</param>
    /// <returns>The deserialized message.</returns>
    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        this.Trace("start: {size} bytes", data.Length);

        var message = JsonSerializer.Deserialize<Message>(data.Span, _messageOpts)!;

        this.Trace("done: {message}", message);

        return message;
    }

    /// <summary>
    /// Serializes an object to binary data using the underlying JSON serializer with detailed debug logging.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data as binary.</returns>
    public ReadOnlyMemory<byte> SerializeData(Type type, object? value)
    {
        var str = _serializer.Serialize(type, value);
        this.Trace<string>("string: {value}", str);

        var raw = Encoding.UTF8.GetBytes(str);
        this.Trace<int, string>("raw({length}): {value}", raw.Length, string.Join(',', raw));

        return raw;
    }

    /// <summary>
    /// Deserializes binary data to an object using the underlying JSON serializer with detailed debug logging.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="data">The binary data to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? DeserializeData(Type type, ReadOnlyMemory<byte> data)
    {
        this.Trace<int, string>("raw({length}): {value}", data.Length, string.Join(',', data.ToArray()));

        var str = Encoding.UTF8.GetString(data.Span);
        this.Trace<string>("string: {value}", str);

        var value = _serializer.Deserialize(type, str);

        return value;
    }
}
