using System;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Extensions;

public static class ModelRefExtensions
{
    public static bool IsFor(this IModelRef @ref, Type type) => @ref switch
    {
        IGenericModelRef x => x.Namespace == type.Namespace &&
            x.Name == type.Name &&
            type.IsGenericType &&
            x.Args.Length == type.GetGenericArguments().Length,
        _ => @ref.Namespace == type.Namespace && @ref.Name == type.Name
    };
}