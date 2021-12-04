using System;
using System.Linq;

namespace Annium.Core.Reflection;

public static class GetOwnInterfacesExtension
{
    public static Type[] GetOwnInterfaces(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type.BaseType == null)
            return type.GetInterfaces();

        var interfaces = type.GetInterfaces();
        var baseInterfaces = type.BaseType.GetInterfaces();

        return interfaces
            .Where(i => !baseInterfaces.Contains(i))
            .OrderBy(i => i.Name)
            .ToArray();
    }
}