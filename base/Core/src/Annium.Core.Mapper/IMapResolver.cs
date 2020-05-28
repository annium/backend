using System;

namespace Annium.Core.Mapper
{
    public interface IMapResolver
    {
        bool CanResolveMap(Type src, Type tgt);

        Mapping ResolveMap(Type src, Type tgt, ResolveMapping resolveMapping);
    }
}