using System;

namespace Annium.Extensions.Mapper
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