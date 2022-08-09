using System;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class GetPropertyOrFieldValueExtension
{
    public static object? GetPropertyOrFieldValue(this MemberInfo member, object target) => member switch
    {
        PropertyInfo property => property.GetGetMethod()?.Invoke(target, Array.Empty<object>())
            ?? throw new InvalidOperationException($"property {member} is not readable"),
        FieldInfo field => field.GetValue(target),
        _ => throw new InvalidOperationException($"{member} is neither readable property nor field")
    };
}