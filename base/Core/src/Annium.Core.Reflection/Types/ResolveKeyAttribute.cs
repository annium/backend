using System;

namespace Annium.Core.Reflection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]

    public class ResolveKeyAttribute : Attribute
    {
        public string Key { get; }

        public ResolveKeyAttribute(string key)
        {
            Key = key;
        }
    }
}