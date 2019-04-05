using System;

namespace Annium.Extensions.Arguments
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class HelpAttribute : BaseAttribute
    {
        public string Help { get; }

        public HelpAttribute(string help)
        {
            Help = help;
        }
    }
}