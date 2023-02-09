using System;
using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Internal.Models;

internal class StructModelBuilder
{
    public static StructModelBuilder Init(Namespace @namespace, string name) => new(@namespace, name);

    private readonly Namespace _namespace;
    private readonly string _name;
    private IReadOnlyList<ModelRef> _genericArguments = Array.Empty<ModelRef>();
    private ModelRef? _base;
    private IReadOnlyList<ModelRef> _interfaces = Array.Empty<ModelRef>();
    private IReadOnlyList<FieldModel> _fields = Array.Empty<FieldModel>();

    private StructModelBuilder(Namespace @namespace, string name)
    {
        _namespace = @namespace;
        _name = name;
    }

    public StructModelBuilder GenericArguments(IReadOnlyList<ModelRef> args)
    {
        _genericArguments = args;

        return this;
    }

    public StructModelBuilder Base(ModelRef baseType)
    {
        _base = baseType;

        return this;
    }

    public StructModelBuilder Interfaces(IReadOnlyList<ModelRef> interfaces)
    {
        _interfaces = interfaces;

        return this;
    }

    public StructModelBuilder Fields(IReadOnlyList<FieldModel> fields)
    {
        _fields = fields;

        return this;
    }

    public StructModel Build() => new(
        _namespace,
        _name,
        _genericArguments,
        _base,
        _interfaces,
        _fields
    );
}