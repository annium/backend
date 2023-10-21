using MessagePack;

namespace Annium.Mesh.Tests.System.Domain;

[MessagePackObject]
public class CounterMessage
{
    [Key(0)]
    public int Value { get; init; }
}