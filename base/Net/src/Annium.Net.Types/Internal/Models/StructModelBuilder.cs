using System;
using System.Collections.Generic;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Internal.Models;

internal class StructModelBuilder
{
    public static StructModelBuilder Init(Namespace @namespace, string name) => new(@namespace, name);

    private readonly Namespace _namespace;
    private readonly string _name;
    private IReadOnlyList<ITypeModel> _genericArguments = Array.Empty<ITypeModel>();
    private StructModel? _base;
    private IReadOnlyList<StructModel> _interfaces = Array.Empty<StructModel>();
    private IReadOnlyList<FieldModel> _fields = Array.Empty<FieldModel>();

    private StructModelBuilder(Namespace @namespace, string name)
    {
        _namespace = @namespace;
        _name = name;
    }

    public StructModelBuilder GenericArguments(IReadOnlyList<ITypeModel> args)
    {
        _genericArguments = args;

        return this;
    }

    public StructModelBuilder Base(StructModel baseType)
    {
        _base = baseType;

        return this;
    }

    public StructModelBuilder Interfaces(IReadOnlyList<StructModel> interfaces)
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