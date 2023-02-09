using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives.Collections.Generic;

namespace Annium.Net.Types.Models;

public sealed record StructModel(
    Namespace Namespace,
    string Name,
    IReadOnlyList<ITypeModel> GenericArguments,
    StructModel? Base,
    IReadOnlyList<StructModel> Interfaces,
    IReadOnlyList<FieldModel> Fields
) : TypeModelBase(Namespace, ResolveName(Name, GenericArguments), ResolveIsGeneric(GenericArguments))
{
    private static string ResolveName(string name, IReadOnlyCollection<ITypeModel> genericArguments) =>
        genericArguments.Count == 0
            ? name
            : $"{name}<{genericArguments.Select(x => x.Name).Join(", ")}>";

    private static bool ResolveIsGeneric(IReadOnlyCollection<ITypeModel> genericArguments) =>
        genericArguments.Any(x => x.IsGeneric);

    public override string ToString() => $"struct {Namespace}.{Name}";
}