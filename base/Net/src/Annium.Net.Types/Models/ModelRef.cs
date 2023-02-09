namespace Annium.Net.Types.Models;

public sealed record ModelRef(string Name)
{
    public override string ToString() => Name;
}