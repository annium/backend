using System;

namespace Annium.Core.Application
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]

    public class ResolveFieldAttribute : Attribute { }
}