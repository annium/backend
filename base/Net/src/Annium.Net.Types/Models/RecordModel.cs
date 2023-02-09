namespace Annium.Net.Types.Models;

public record RecordModel(ITypeModel Key, ITypeModel Value) : ITypeModel
{
    public string Name { get; } = $"Record<{Key}, {Value}>";
    public override string ToString() => Name;
}