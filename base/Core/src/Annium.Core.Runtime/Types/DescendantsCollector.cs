using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Core.Runtime.Types
{
    internal class DescendantsCollector
    {
        public IDictionary<Type, Type[]> CollectDescendants(Type[] types)
        {
            var result = new Dictionary<Type, Type[]>();
            foreach (var type in types)
            {
                var descendants = CollectDescendants(types, type);
                if (descendants.Length > 0)
                    result[type] = descendants;
            }

            return result;
        }

        private Type[] CollectDescendants(Type[] types, Type type)
        {
            // can have descendants if type is interface or class
            if (!type.IsInterface && !type.IsClass)
                return Type.EmptyTypes;

            if (!type.IsGenericTypeDefinition)
                return types
                    .Where(IsAssignableFrom(type))
                    .ToArray();

            if (type.IsInterface)
                return types.Where(IsGenericInterfaceImplementation(type)).ToArray();

            return types
                .Where(IsGenericClassExtension(type))
                .ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<Type, bool> IsAssignableFrom(Type type) => x =>
            IsOtherNonAbstractClass(type, x) &&
            type.IsAssignableFrom(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<Type, bool> IsGenericInterfaceImplementation(Type type) => x =>
            IsOtherNonAbstractClass(type, x) &&
            x.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<Type, bool> IsGenericClassExtension(Type type) => x =>
        {
            if (!IsOtherNonAbstractClass(type, x))
                return false;

            if (x.BaseType == null || x.BaseType == typeof(object))
                return false;

            while (x != null)
            {
                if (x.IsGenericType && x.GetGenericTypeDefinition() == type)
                    return true;

                x = x.BaseType!;
            }

            return false;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOtherNonAbstractClass(Type type, Type x) =>
            x != type &&
            x.IsClass &&
            !x.IsAbstract;
    }
}