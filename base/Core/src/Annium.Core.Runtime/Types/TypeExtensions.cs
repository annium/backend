using System;

namespace Annium.Core.Runtime.Types
{
    public static class TypeExtensions
    {
        public static int GetId(this Type type) => HashCode.Combine(
            type.Assembly.FullName,
            type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericTypeDefinition().FullName : type.FullName
        );
    }
}