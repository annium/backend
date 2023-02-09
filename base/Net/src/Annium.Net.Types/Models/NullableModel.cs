namespace Annium.Net.Types.Models;

public record NullableModel(ITypeModel Type) : ITypeModel
{
    public string Name { get; } = $"{Type.Name}?";
    public override string ToString() => Name;
}