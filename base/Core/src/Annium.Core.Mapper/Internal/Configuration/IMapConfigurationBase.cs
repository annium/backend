using System;

namespace Annium.Core.Mapper.Internal
{
    internal interface IMapConfigurationBase
    {
        Delegate? Type { get; }
    }
}