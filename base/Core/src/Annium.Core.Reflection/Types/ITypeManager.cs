using System;

namespace Annium.Core.Reflection
{
    public interface ITypeManager
    {
        Type? GetByName(string name);
        bool CanResolve(Type baseType);
        Type[] GetImplementations(Type baseType);
        Type? ResolveBySignature(object instance, Type baseType, bool exact);
        Type? ResolveBySignature(string[] signature, Type baseType, bool exact);
        Type? ResolveByKey(string key, Type baseType);
    }
}