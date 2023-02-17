using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Primitives;

public static class TypeBaseExtensions
{
    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
    }
}