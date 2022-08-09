using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Reflection.Extensions;

namespace Annium.Core.Reflection;

public static class GetAllMethodsExtension
{
    public static MethodInfo[] GetAllMethods(this Type type) =>
        type.GetAllMethods(Constants.DefaultBindingFlags);

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