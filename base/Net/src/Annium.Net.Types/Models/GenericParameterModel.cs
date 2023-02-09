namespace Annium.Net.Types.Models;

public sealed record GenericParameterModel(string Name) : ITypeModel
{
    public override string ToString() => Name;
    public bool IsGeneric => true;
}