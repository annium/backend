namespace Annium.Net.Types.Models;

public sealed record ArrayModel(ITypeModel Type) : ITypeModel
{
    public string Name { get; } = $"{Type}[]";
    public bool IsGeneric => Type.IsGeneric;
    public override string ToString() => Name;
}