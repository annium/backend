using System;

namespace Annium.Core.Mapper
{
    public interface IMappingContext
    {
        Delegate GetMap(Type src, Type tgt);

        Mapping ResolveMapping(Type src, Type tgt);
    }
}