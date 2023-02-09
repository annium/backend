namespace Annium.Net.Types.Models;

public sealed record FieldModel(
    ITypeModel Type,
    string Name
)
{
    public override string ToString() => $"{Type} {Name}";
}