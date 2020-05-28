using System;

namespace Annium.Core.Mapper.Internal
{
    internal interface IMapBuilder
    {
        bool HasMap(Type src, Type tgt);

        Delegate GetMap(Type src, Type tgt);
    }
}