using System;

namespace Annium.Core.Runtime.Types
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ResolveFieldAttribute : Attribute
    {
    }
}