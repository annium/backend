using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Extensions;

internal static class ContextualExtensions
{
    public static IReadOnlyCollection<ContextualType> GetInterfaces(this ContextualType type) => type.Type.GetInterfaces()
        .Select(x => x.ToContextualType())
        .ToArray();

    public static IReadOnlyCollection<ContextualAccessorInfo> GetMembers(this ContextualType type) => type.GetProperties()
        .Concat(type.GetFields())
        .Select(x => x.ToContextualAccessor())
        .ToArray();

    private static IReadOnlyCollection<MemberInfo> GetProperties(this ContextualType type) => type.Type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance);

    private static IReadOnlyCollection<MemberInfo> GetFields(this ContextualType type) => type.Type
        .GetFields(BindingFlags.Public | BindingFlags.Instance);
}