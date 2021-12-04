using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class GetAllPropertiesExtension
{
    public static PropertyInfo[] GetAllProperties(
        this Type type
    )
    {
        var info = type.GetTypeInfo();

        return info.GetProperties()
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetProperties()))
            .ToArray();
    }

    public static PropertyInfo[] GetAllProperties(
        this Type type,
        BindingFlags flags
    )
    {
        var info = type.GetTypeInfo();

        return info.GetProperties(flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetProperties(flags)))
            .ToArray();
    }
}