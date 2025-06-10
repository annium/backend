using MessagePack;

namespace Annium.Mesh.Tests.System.Domain;

/// <summary>
/// Represents a counter message containing an integer value, used for mesh communication testing.
/// </summary>
[MessagePackObject]
public class CounterMessage
{
    /// <summary>
    /// Gets or sets the integer value of the counter message.
    /// </summary>
    [Key(0)]
    public int Value { get; init; }
}
