namespace Annium.Net.Types.Models;

public sealed record ModelRef(string Name) : ITypeModel
{
    public bool IsGeneric => false;
}