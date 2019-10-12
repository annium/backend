using System;
using System.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal abstract class FieldConfigurationBase : IFieldConfigurationBase
    {
        /// <summary>
        /// Destination property, this field configuration is related to. May be on nested level
        /// </summary>
        public PropertyInfo Property { get; }
        /// <summary>
        /// Map, built for property
        /// </summary>
        public Delegate Map { get; private set; } = null!;

        public FieldConfigurationBase(
            PropertyInfo property
        )
        {
            Property = property;
        }
    }
}