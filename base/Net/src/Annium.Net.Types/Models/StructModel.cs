using System;
using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

public sealed record StructModel(
    Namespace Namespace,
    string Name
) : ModelBase(Namespace, Name)
{
    public IReadOnlyList<IRef> Args { get; private set; } = Array.Empty<IRef>();
    public IRef? Base { get; private set; }
    public IReadOnlyList<IRef> Interfaces { get; private set; } = Array.Empty<IRef>();
    public IReadOnlyList<FieldModel> Fields { get; private set; } = Array.Empty<FieldModel>();

    public void SetArgs(IReadOnlyList<IRef> args)
    {
        Args = args;
    }

    public void SetBase(IRef @base)
    {
        Base = @base;
    }

    public void SetInterfaces(IReadOnlyList<IRef> interfaces)
    {
        Interfaces = interfaces;
    }

    public void SetFields(IReadOnlyList<FieldModel> fields)
    {
        Fields = fields;
    }

    public override string ToString() => $"struct {Namespace}.{Name}";
}