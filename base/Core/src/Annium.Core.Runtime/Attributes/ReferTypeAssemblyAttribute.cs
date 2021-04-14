using System;

namespace Annium.Core.Runtime
{
    /// <summary>
    /// By default, CLR doesn't load assemblies, not referenced directly from code.
    /// This hack is to overcome such limitation
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ReferTypeAssemblyAttribute : Attribute
    {
        private readonly Type _type;

        public ReferTypeAssemblyAttribute(Type type)
        {
            _type = type;
        }
    }
}