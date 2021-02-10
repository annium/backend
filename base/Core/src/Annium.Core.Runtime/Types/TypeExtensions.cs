using System;

namespace Annium.Core.Runtime.Types
{
    public static class TypeExtensions
    {
        public static TypeId GetId(this Type type) => TypeId.Create(type);
        public static string GetIdString(this Type type) => TypeId.Create(type).Id;
    }
}