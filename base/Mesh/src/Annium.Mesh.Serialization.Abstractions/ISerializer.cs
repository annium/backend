using System;

namespace Annium.Mesh.Serialization.Abstractions;

public interface ISerializer
{
    ReadOnlyMemory<byte> Serialize<T>(T value);
    T Deserialize<T>(ReadOnlyMemory<byte> data);
}