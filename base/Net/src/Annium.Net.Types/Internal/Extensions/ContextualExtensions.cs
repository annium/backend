using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Extensions;

internal static class ContextualExtensions
{
    public static bool IsNullable(this ContextualType type) => type.Nullability is not Nullability.NotNullable;
    public static bool IsNullable(this ContextualPropertyInfo property) => property.Nullability is not Nullability.NotNullable;
    public static bool IsNullable(this ContextualFieldInfo field) => field.Nullability is not Nullability.NotNullable;
}