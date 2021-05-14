using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AutoScannedAttribute : Attribute
    {
        public Assembly[] Dependencies { get; }

        public AutoScannedAttribute(params Type[] dependencies)
        {
            Dependencies = dependencies.Select(x => x.Assembly).Distinct().ToArray();
        }
    }
}