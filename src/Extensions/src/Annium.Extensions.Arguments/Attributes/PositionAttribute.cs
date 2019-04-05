using System;

namespace Annium.Extensions.Arguments
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PositionAttribute : BaseAttribute
    {
        public int Position { get; }

        public bool IsRequired { get; }

        public PositionAttribute(
            int position,
            bool isRequired = true
        )
        {
            Position = position;
            IsRequired = isRequired;
        }
    }
}