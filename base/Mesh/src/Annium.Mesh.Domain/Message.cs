using System;

namespace Annium.Mesh.Domain;

public sealed class Message
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public uint Version { get; init; }
    public MessageType Type { get; init; }
    public uint Code { get; init; }
    public ReadOnlyMemory<byte> Data { get; init; } = Array.Empty<byte>();
}