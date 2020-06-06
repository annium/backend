using System;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    internal sealed class Descendant
    {
        public Type Type { get; }
        public TypeSignature Signature { get; }
        public object? Key { get; }
        public bool HasKey { get; }

        public Descendant(
            Type type
        )
        {
            Type = type;
            Signature = TypeSignature.Create(type);
            var keyAttribute = type.GetTypeInfo().GetCustomAttribute<ResolutionKeyValueAttribute>();
            Key = keyAttribute?.Key;
            HasKey = keyAttribute != null;
        }

        public override string ToString() => Type.Name;
    }
}