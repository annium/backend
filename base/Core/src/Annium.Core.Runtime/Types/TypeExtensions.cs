using System;
using Annium.Core.Primitives;

namespace Annium.Core.Runtime.Types
{
    public static class TypeExtensions
    {
        public static string GetId(this Type type)
        {
            var assembly = type.Assembly.GetName().Name;
            var name = type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetGenericTypeDefinition().FriendlyName() : type.FriendlyName();

            return $"{assembly}.{name}";
        }
    }
}