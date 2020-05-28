using System;

namespace Annium.Core.Mapper
{
    public interface IConfigurableMapResolver
    {
        int Order { get; }

        bool CanResolveMap(Type src, Type tgt);

        Mapping ResolveMap(Type src, Type tgt, Map cfg, IMappingContext ctx);
    }
}