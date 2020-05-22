using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Runtime.Types
{
    internal class DescendantsCollector
    {
        private delegate void RegisterAncestor(Type type, Type ancestor);

        public IReadOnlyDictionary<Type, HashSet<Type>> CollectDescendants(HashSet<Type> types)
        {
            var result = new Dictionary<Type, HashSet<Type>>();

            // only classes and interfaces can have/be descendants
            var classes = types.Where(x => x.IsClass).ToArray();
            foreach (var type in classes)
                CollectClassAncestors(type, Register);

            var interfaces = types.Where(x => x.IsInterface).ToArray();
            foreach (var type in interfaces)
                CollectInterfaceAncestors(type, Register);

            return result;

            void Register(Type type, Type ancestor)
            {
                if (ancestor.IsGenericType)
                    ancestor = ancestor.GetGenericTypeDefinition();

                if (result.TryGetValue(ancestor, out var descendants))
                    descendants.Add(type);
                else
                    result[ancestor] = new HashSet<Type> { type };
            }
        }

        private void CollectClassAncestors(Type type, RegisterAncestor register)
        {
            foreach (var ancestor in type.GetInterfaces())
                register(type, ancestor);

            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                register(type, baseType);
                baseType = baseType.BaseType;
            }
        }

        private void CollectInterfaceAncestors(Type type, RegisterAncestor register)
        {
            foreach (var ancestor in type.GetInterfaces())
                register(type, ancestor);
        }
    }
}