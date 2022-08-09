using System;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class PropertyOrFieldMemberExtensions
{
    public static object? GetPropertyOrFieldValue(this MemberInfo member, object target) => member switch
    {
        PropertyInfo property => property.GetGetMethod()?.Invoke(target, Array.Empty<object>())
            ?? throw new InvalidOperationException($"property {member} is not readable"),
        FieldInfo field => field.GetValue(target),
        _ => throw new InvalidOperationException($"{member} is neither readable property nor field")
    };

    public static void SetPropertyOrFieldValue(this MemberInfo member, object target, object? value)
    {
        if (member is PropertyInfo property)
        {
            var setter = property.GetSetMethod();
            if (setter is null)
                throw new InvalidOperationException($"property {member} is not writable");
            setter.Invoke(target, new[] { value });
        }

        else if (member is FieldInfo field)
            field.SetValue(target, value);

        else
            throw new InvalidOperationException($"{member} is neither readable property nor field");
    }
}