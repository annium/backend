using System;
using Annium.Mesh.Domain;

namespace Annium.Mesh.Serialization.Abstractions;

public interface ISerializer
{
    ReadOnlyMemory<byte> SerializeMessage(Message message);
    Message DeserializeMessage(ReadOnlyMemory<byte> data);
    ReadOnlyMemory<byte> SerializeData(Type type, object? value);
    object? DeserializeData(Type type, ReadOnlyMemory<byte> data);
}
