using System;

namespace Annium.Core.Reflection
{
    public static class HasDefaultConstructorExtension
    {
        public static bool HasDefaultConstructor(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsClass)
                return type.GetConstructor(Type.EmptyTypes) != null;

            if (type.IsValueType)
                return type.GetConstructors().Length == 0 || type.GetConstructor(Type.EmptyTypes) != null;

            throw new ArgumentException($"{type} is not constructable");
        }
    }
}