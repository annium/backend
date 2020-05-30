using System;

namespace Annium.Core.Runtime.Types
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ResolutionKeyValueAttribute : Attribute
    {
        public string Key { get; }

        public ResolutionKeyValueAttribute(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            Key = key;
        }
    }
}