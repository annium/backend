using System;
using Annium.Mesh.Domain;

namespace Annium.Mesh.Serialization.Abstractions;

public interface ISerializer
{
    ReadOnlyMemory<byte> SerializeMessage(Message message);
    Message DeserializeMessage(ReadOnlyMemory<byte> data);
    ReadOnlyMemory<byte> SerializeData<T>(T value);
    object? DeserializeData(ReadOnlyMemory<byte> data, Type type);
}