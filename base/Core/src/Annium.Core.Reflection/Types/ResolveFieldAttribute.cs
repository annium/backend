using System;

namespace Annium.Core.Reflection
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]

    public class ResolveFieldAttribute : Attribute { }
}