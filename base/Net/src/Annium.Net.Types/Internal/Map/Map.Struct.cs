using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static ITypeModel ToStruct(Type type)
    {
        var contextType = type.ToContextualType();
        var name = type.FriendlyName();
        if (type.IsGenericType)
            name = name[..name.IndexOf('<')];
        var builder = StructModelBuilder.Init(type.GetNamespace(), name);

        var genericArguments = contextType.GenericArguments.Select(ToModel).ToArray();
        builder.GenericArguments(genericArguments);

        if (contextType.BaseType is not null && !IsIgnoredType(contextType.BaseType))
            builder.Base((StructModel) ToModel(contextType.BaseType));

        var interfaces = type.GetInterfaces()
            .Select(x => x.ToContextualType())
            .Where(x => !IsIgnoredType(x))
            .Select(ToModel)
            .OfType<StructModel>()
            .ToArray();
        builder.Interfaces(interfaces);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new FieldModel(ToModel(x.ToContextualProperty().PropertyType), x.Name))
            .ToArray();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new FieldModel(ToModel(x.ToContextualField().FieldType), x.Name))
            .ToArray();
        builder.Fields(properties.Concat(fields).ToArray());

        var model = builder.Build();
        Console.WriteLine($"Mapped {type} -> {model}");

        return model;
    }
}