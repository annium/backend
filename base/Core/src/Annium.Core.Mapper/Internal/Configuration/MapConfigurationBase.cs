using System;
using System.Collections.Generic;

namespace Annium.Core.Mapper.Internal
{
    internal class MapConfigurationBase : IMapConfigurationBase
    {
        /// <summary>
        /// Delegate, implementing complete Type conversion
        /// </summary>
        public Delegate? Type { get; set; }
        /// <summary>
        /// Fields configuration
        /// </summary>
        public HashSet<IFieldConfigurationBase> Fields = new HashSet<IFieldConfigurationBase>();
    }
}