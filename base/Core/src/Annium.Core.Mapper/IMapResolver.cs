using System;

namespace Annium.Core.Mapper
{
    public interface IMapResolver
    {
        int Order { get; }

        bool CanResolveMap(Type src, Type tgt);

        Mapping ResolveMap(Type src, Type tgt, IMappingContext ctx);
    }
}