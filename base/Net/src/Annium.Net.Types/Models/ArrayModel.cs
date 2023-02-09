namespace Annium.Net.Types.Models;

public sealed record ArrayModel(ITypeModel Type) : ITypeModel
{
    public string Name { get; } = $"{Type}[]";
    public override string ToString() => Name;
}