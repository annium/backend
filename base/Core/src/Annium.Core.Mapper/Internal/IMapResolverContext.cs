using System;

namespace Annium.Core.Mapper.Internal
{
    public interface IMapResolverContext
    {
        Delegate GetMap(Type src, Type tgt);

        Mapping ResolveMapping(Type src, Type tgt);
    }
}