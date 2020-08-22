using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types
{
    internal sealed class Ancestor
    {
        public Type Type { get; }
        public PropertyInfo? KeyProperty { get; }
        public bool HasKeyProperty { get; }

        public Ancestor(
            Type type
        )
        {
            var keyProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ResolutionKeyAttribute>() != null)
                .ToArray();

            if (keyProperties.Length > 1)
                throw new ArgumentException($"Type '{type}' has multiple resolution keys defined: {string.Join(", ", keyProperties.Select(f => f.Name))}.");

            Type = type;
            KeyProperty = keyProperties.FirstOrDefault();
            HasKeyProperty = KeyProperty != null;
        }

        public override string ToString() => Type.Name;
    }
}