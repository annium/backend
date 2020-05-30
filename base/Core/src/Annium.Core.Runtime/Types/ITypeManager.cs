using System;
using System.Collections.Generic;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    public interface ITypeManager
    {
        IReadOnlyCollection<Type> Types { get; }
        bool HasImplementations(Type baseType);
        Type[] GetImplementations(Type baseType);
        PropertyInfo? GetResolutionKeyProperty(Type baseType);
        Type? ResolveByKey(string key, Type baseType);
        Type? ResolveBySignature(IEnumerable<string> signature, Type baseType, bool exact = false);
        Type? Resolve(object instance, Type baseType);
    }
}