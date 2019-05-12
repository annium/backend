using System;

namespace Annium.Extensions.Mapper
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]

    public class ResolveFieldAttribute : Attribute { }
}