using System;

namespace Annium.Mesh.Domain;

public sealed class Message
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ushort Version { get; init; }
    public MessageType Type { get; init; }
    public int Action { get; init; }
    public ReadOnlyMemory<byte> Data { get; init; } = Array.Empty<byte>();

    public override string ToString() => $"v{Version}; {Type}.{Action} | {Data.Length}";
}
