using System;
using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

public sealed record InterfaceModel(
    Namespace Namespace,
    string Name
) : IGenericModel
{
    public IReadOnlyList<GenericParameterRef> Args { get; private set; } = Array.Empty<GenericParameterRef>();
    public IReadOnlyList<InterfaceRef> Interfaces { get; private set; } = Array.Empty<InterfaceRef>();
    public IReadOnlyList<FieldModel> Fields { get; private set; } = Array.Empty<FieldModel>();

    public void SetArgs(IReadOnlyList<GenericParameterRef> args)
    {
        Args = args;
    }

    public void SetInterfaces(IReadOnlyList<InterfaceRef> interfaces)
    {
        Interfaces = interfaces;
    }

    public void SetFields(IReadOnlyList<FieldModel> fields)
    {
        Fields = fields;
    }

    public override string ToString() => $"interface {Namespace}.{Name}";
}