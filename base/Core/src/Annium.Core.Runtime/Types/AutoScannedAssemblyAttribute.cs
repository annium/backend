using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AutoScannedAssemblyAttribute : Attribute
    {
        public Assembly[] Dependencies { get; }

        public AutoScannedAssemblyAttribute(params Type[] dependencies)
        {
            Dependencies = dependencies.Select(x => x.Assembly).Distinct().ToArray();
        }
    }
}