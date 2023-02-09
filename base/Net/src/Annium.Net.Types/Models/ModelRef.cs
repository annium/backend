namespace Annium.Net.Types.Models;

public sealed record ModelRef(string Namespace, string Name)
{
    public bool HasNamespace { get; } = true;

    public ModelRef(string name) : this(string.Empty, name)
    {
        HasNamespace = false;
    }

    public override string ToString() => HasNamespace ? $"{Namespace}.{Name}" : Name;
}