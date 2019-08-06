using System;

namespace Annium.Extensions.Arguments
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ValuesAttribute : BaseAttribute
    {
        public string[] Values { get; }

        public ValuesAttribute(params string[] values)
        {
            Values = values;
        }
    }
}