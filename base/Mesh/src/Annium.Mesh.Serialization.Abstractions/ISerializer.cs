using System;
using Annium.Mesh.Domain;

namespace Annium.Mesh.Serialization.Abstractions;

/// <summary>
/// Provides serialization and deserialization functionality for mesh messages and data payloads.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Serializes a mesh message to binary format.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The serialized message as binary data.</returns>
    ReadOnlyMemory<byte> SerializeMessage(Message message);

    /// <summary>
    /// Deserializes binary data back to a mesh message.
    /// </summary>
    /// <param name="data">The binary data to deserialize.</param>
    /// <returns>The deserialized message.</returns>
    Message DeserializeMessage(ReadOnlyMemory<byte> data);

    /// <summary>
    /// Serializes an object of the specified type to binary format.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data as binary.</returns>
    ReadOnlyMemory<byte> SerializeData(Type type, object? value);

    /// <summary>
    /// Deserializes binary data to an object of the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="data">The binary data to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    object? DeserializeData(Type type, ReadOnlyMemory<byte> data);
}
