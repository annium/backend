using System;

namespace Annium.Core.Mapper
{
    public interface IConfigurableMapResolver
    {
        bool CanResolveMap(Type src, Type tgt);

        Mapping ResolveMap(Type src, Type tgt, Map cfg, IMappingContext ctx);
    }
}