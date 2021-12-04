using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class GetAllMethodsExtension
{
    public static MethodInfo[] GetAllMethods(
        this Type type
    )
    {
        var info = type.GetTypeInfo();

        return info.GetMethods()
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetMethods()))
            .ToArray();
    }

    public static MethodInfo[] GetAllMethods(
        this Type type,
        BindingFlags flags
    )
    {
        var info = type.GetTypeInfo();

        return info.GetMethods(flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetMethods(flags)))
            .ToArray();
    }
}