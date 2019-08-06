using System;

namespace Annium.Core.Application.Types
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]

    public class ResolveFieldAttribute : Attribute { }
}