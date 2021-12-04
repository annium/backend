using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class GetAllFieldsExtension
{
    public static FieldInfo[] GetAllFields(
        this Type type
    )
    {
        var info = type.GetTypeInfo();

        return info.GetFields()
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetFields()))
            .ToArray();
    }

    public static FieldInfo[] GetAllFields(
        this Type type,
        BindingFlags flags
    )
    {
        var info = type.GetTypeInfo();

        return info.GetFields(flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetFields(flags)))
            .ToArray();
    }
}