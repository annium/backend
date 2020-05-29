using System;
using System.Collections.Generic;

namespace Annium.Core.Runtime.Types
{
    public interface ITypeManager
    {
        IReadOnlyCollection<Type> Types { get; }
        Type? GetByName(string fullName);
        bool CanResolve(Type baseType);
        Type[] GetImplementations(Type baseType);
        Type? ResolveBySignature(object instance, Type baseType, bool exact);
        Type? ResolveBySignature(string[] signature, Type baseType, bool exact);
        Type? ResolveByKey(string key, Type baseType);
    }
}