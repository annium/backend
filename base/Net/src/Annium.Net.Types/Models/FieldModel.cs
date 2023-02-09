namespace Annium.Net.Types.Models;

public sealed record FieldModel(
    ModelRef Type,
    string Name
)
{
    public override string ToString() => $"{Type} {Name}";
}